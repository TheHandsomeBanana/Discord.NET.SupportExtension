using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text;
using Microsoft;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Collections.Immutable;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft.VisualStudio.Core.Imaging;
using Discord.NET.SupportExtension.Mef;
using HB.NETF.VisualStudio.Workspace;

namespace Discord.NET.SupportExtension.MEF.CompletionSource {
    internal class AsyncDiscordCompletionSource : IAsyncCompletionSource {
        private bool _isDisposed;
        public VisualStudioWorkspace VSWorkspace { get; set; }
        public DocumentId DocumentIdentifier { get; set; }

        public AsyncDiscordCompletionSource() {
            VSWorkspace = WorkspaceHelper.GetVisualStudioWorkspace();
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token) {
            Microsoft.CodeAnalysis.Document document = VSWorkspace.CurrentSolution.GetDocument(DocumentIdentifier);
            if (document == null)
                return default;

            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            if (loggerFactory == null) // Package not loaded => Nullref
                return default;

            ILogger<AsyncDiscordCompletionSource> logger = loggerFactory.GetOrCreateLogger<AsyncDiscordCompletionSource>();
            IAsyncDiscordCompletionEngine engine = DIContainer.GetService<IAsyncDiscordCompletionEngine>();

            try {
                Assumes.Present(engine);
                Stopwatch stopwatch = Stopwatch.StartNew();
                SemanticModel semanticModel = await document.GetSemanticModelAsync(token);
                SyntaxToken triggerToken = (await document.GetSyntaxRootAsync(token)).FindToken(triggerLocation);

                IDiscordCompletionItem[] completions = await engine.ProcessCompletionAsync(VSWorkspace.CurrentSolution, semanticModel, triggerToken);

                if (completions.Length > 0)
                    logger.LogInformation($"{completions.Length} completions added.");

                logger.LogInformation($"Completion finished in {stopwatch.ElapsedMilliseconds} ms.");

                stopwatch.Stop();
                return new CompletionContext(completions.Select(e =>
                    new CompletionItem(e.DisplayText, this, DiscordImage, DiscordFilters, e.Suffix, e.InsertText, e.InsertText, e.InsertText, DiscordImageArray)).ToImmutableArray()
                );
            }
            catch (Exception ex) {
                logger.LogCritical("Completion context engine failed. " + ex.ToString());
            }

            return default;
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) {
            if (item.Properties.ContainsProperty("description"))
                return await Task.FromResult(item.Properties["description"] as ContainerElement);
            return null;
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token) {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (DocumentIdentifier == null) {
                DTE dte = WorkspaceHelper.GetDTE();
                if (dte?.ActiveDocument != null)
                    DocumentIdentifier = VSWorkspace.CurrentSolution.GetDocumentIdsWithFilePath(dte.ActiveDocument.FullName).FirstOrDefault();
            }

            return CompletionStartData.ParticipatesInCompletionIfAny;
        }

        public void Dispose() {
            if (!_isDisposed) {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private static readonly ImageElement DiscordImage = new ImageElement(new ImageId(PackageImageIds.discordImages, PackageImageIds.bmpPic1), "Discord");
        private static readonly ImmutableArray<ImageElement> DiscordImageArray = new ImageElement[] { new ImageElement(new ImageId(PackageImageIds.discordImages, PackageImageIds.bmpPic1), "Discord") }.ToImmutableArray();
        private static readonly ImmutableArray<CompletionFilter> DiscordFilters = new CompletionFilter[] { new CompletionFilter("Discord", "D", DiscordImage) }.ToImmutableArray();
    }
}