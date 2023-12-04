using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Mef;
using EnvDTE;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Unity;
using HB.NETF.VisualStudio.Workspace;
using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity;

namespace Discord.NET.SupportExtension.MEF.CompletionSource {
    public class AsyncDiscordCompletionSource : IAsyncCompletionSource {
        private bool _isDisposed;
        private DocumentId documentIdentifier;
        private readonly VisualStudioWorkspace vsWorkspace;
        public AsyncDiscordCompletionSource() {
            vsWorkspace = WorkspaceHelper.VisualStudioWorkspace;
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token) {
            IUnityContainer supportPackageContainer;

            try {
                supportPackageContainer = UnityBase.GetChildContainer("DiscordSupportPackage");
            }
            catch {
                return default;
            }

            ILoggerFactory loggerFactory = supportPackageContainer.Resolve<ILoggerFactory>();
            if (loggerFactory == null) // Package not loaded => Nullref
                return default;

            ILogger<AsyncDiscordCompletionSource> logger = loggerFactory.GetOrCreateLogger<AsyncDiscordCompletionSource>();
            IDiscordCompletionEngine engine = supportPackageContainer.Resolve<IDiscordCompletionEngine>();

            try {
                if (token.IsCancellationRequested) {
                    logger.LogInformation("Completion cancelled");
                    return default;
                }

                Assumes.Present(engine);
                Stopwatch stopwatch = Stopwatch.StartNew();
                Microsoft.CodeAnalysis.Document document = vsWorkspace.CurrentSolution.GetDocument(documentIdentifier);
                if (document == null)
                    return default;

                SemanticModel semanticModel = await document.GetSemanticModelAsync(token);
                SyntaxToken triggerToken = (await document.GetSyntaxRootAsync(token)).FindToken(triggerLocation);

                DiscordCompletionItem[] completions = await engine.ProcessCompletionAsync(vsWorkspace.CurrentSolution, semanticModel, triggerToken);

                if (completions.Length > 0)
                    logger.LogInformation($"{completions.Length} completions added in {stopwatch.ElapsedMilliseconds} ms.");

                stopwatch.Stop();
                return new CompletionContext(completions.Select(e => {
                    CompletionItem ci = new CompletionItem(e.DisplayText, this, discordImage, discordFilters, e.Suffix, e.InsertText, e.DisplayText, e.DisplayText, ImmutableArray<ImageElement>.Empty);
                    ci.Properties.AddProperty("description", e.Description);
                    return ci;
                }).ToImmutableArray());
            }
            catch (Exception ex) {
                logger.LogCritical("Completion context engine failed. " + ex.ToString());
            }

            return default;
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) {
            if (item.Properties.ContainsProperty("description"))
                return await Task.FromResult(item.Properties["description"] as ContainerElement);
            return await Task.FromResult("");
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token) {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (documentIdentifier == null) {
                DTE dte = WorkspaceHelper.GetDTE();
                if (dte?.ActiveDocument != null)
                    documentIdentifier = vsWorkspace.CurrentSolution.GetDocumentIdsWithFilePath(dte.ActiveDocument.FullName).FirstOrDefault();
            }

            return CompletionStartData.ParticipatesInCompletionIfAny;
        }

        public void Dispose() {
            if (!_isDisposed) {
                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private static readonly ImageElement discordImage = new ImageElement(new ImageId(PackageImageIds.DiscordMoniker, PackageImageIds.Discord), "Discord");
        private static readonly ImmutableArray<CompletionFilter> discordFilters = new CompletionFilter[] { new CompletionFilter("Discord", "D", discordImage) }.ToImmutableArray();
    }
}