using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;

namespace Discord.NET.SupportExtension.Mef.Trigger {
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class TextViewCreationListener : ITextViewCreationListener {
        [Import]
        internal IAsyncCompletionBroker CompletionBroker { get; set; }

        public void TextViewCreated(ITextView textView) {
            textView.TextBuffer.Changed += (sender, args) => TextBufferChanged(textView, args);
        }

        private void TextBufferChanged(ITextView textView, TextContentChangedEventArgs e) {
            foreach (var change in e.Changes) {
                if (change.NewText.Length == 1 && char.IsDigit(change.NewText[0])) {
                    ThreadHelper.JoinableTaskFactory.Run(async () => {

                        ITextSnapshot snapshot = e.After;

                        var triggerPoint = new SnapshotPoint(snapshot, change.NewPosition);
                        var trigger = new CompletionTrigger(CompletionTriggerReason.Invoke, snapshot, triggerPoint.GetChar());


                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        CompletionBroker?.TriggerCompletion(textView, trigger, triggerPoint, CancellationToken.None);
                    });
                }
            }
        }
    }
}
