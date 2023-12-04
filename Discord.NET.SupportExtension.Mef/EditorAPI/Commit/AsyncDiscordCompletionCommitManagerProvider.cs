using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Discord.NET.SupportExtension.Mef.Commit {
    [Export(typeof(IAsyncCompletionCommitManagerProvider))]
    [ContentType("CSharp")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [Order(Before = "default")]
    [Name("Discord Commit Provider")]
    public class AsyncDiscordCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider {
        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView) {
            return new AsyncDiscordCompletionCommitManager();
        }
    }
}
