using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
