using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.MEF {
    public class AsyncDiscordCompletionCommitManagerProvider : IAsyncCompletionCommitManagerProvider {
        public IAsyncCompletionCommitManager GetOrCreate(ITextView textView) {
            return new AsyncDiscordCompletionCommitManager();
        }
    }
}
