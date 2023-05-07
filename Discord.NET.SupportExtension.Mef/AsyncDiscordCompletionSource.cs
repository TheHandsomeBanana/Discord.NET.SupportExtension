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

namespace Discord.NET.SupportExtension.MEF {
    internal class AsyncDiscordCompletionSource : IAsyncCompletionSource {
        private bool _isDisposed;
        public VisualStudioWorkspace VSWorkspace { get; set; }
        public DocumentId DocumentIdentifier { get; set; }

        public AsyncDiscordCompletionSource() {
            IComponentModel componentModel = (IComponentModel)Package.GetGlobalService(typeof(SComponentModel));
            VSWorkspace = componentModel.GetService<VisualStudioWorkspace>();
        }

        public async Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token) {
            Microsoft.CodeAnalysis.Document document = VSWorkspace.CurrentSolution.GetDocument(DocumentIdentifier);
            if (document == null)
                return default;

            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            if (loggerFactory == null) // LoggerFactory not present => Extension crash
                return default;

            ILogger<AsyncDiscordCompletionSource> logger = loggerFactory.CreateLogger<AsyncDiscordCompletionSource>();

            IAsyncDiscordCompletionEngine engine = DIContainer.ServiceProvider.GetService(typeof(IAsyncDiscordCompletionEngine)) as IAsyncDiscordCompletionEngine;            

            try {
                Assumes.Present(engine);
                SemanticModel semanticModel = await document.GetSemanticModelAsync(token);
                SyntaxToken triggerToken = (await document.GetSyntaxRootAsync(token)).FindToken(triggerLocation);

                IDiscordCompletionItem[] completions = await engine.ProcessCompletionAsync(VSWorkspace.CurrentSolution, semanticModel, triggerToken);

                return new CompletionContext(completions.Select(e => new CompletionItem(GetCompletionItemDisplayName(e), this)).ToImmutableArray());
            }
            catch (Exception ex) {
                logger.LogCritical("IntelliSense adaption failed. " + ex.ToString());
            }

            return default;
        }

        public async Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token) {
            return await Task.FromResult(item.Properties["description"] as ContainerElement);
        }

        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token) {
            ThreadHelper.ThrowIfNotOnUIThread(); // DTE access should only be done in main thread
            if (DocumentIdentifier == null) {
                _DTE dte = Package.GetGlobalService(typeof(DTE)) as _DTE;
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

        private string GetCompletionItemDisplayName(IDiscordCompletionItem item) => $"{item.Name} [{item.Id}] ({item.CompletionContext})";

    }
}
