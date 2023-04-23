using EnvDTE;
using Microsoft.Build.Framework;
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
            
            
            List<string> strList = new List<string>();
            strList.Add("addition");
            strList.Add("adaptation");
            strList.Add("subtraction");
            strList.Add("summation");

            return new CompletionContext(strList.Select(e => new CompletionItem(e, this)).ToImmutableArray());
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
    }
}
