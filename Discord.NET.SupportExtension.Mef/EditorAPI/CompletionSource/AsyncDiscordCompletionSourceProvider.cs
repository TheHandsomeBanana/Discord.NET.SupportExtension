using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace Discord.NET.SupportExtension.MEF.CompletionSource {
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType("CSharp")]
    [Name("Discord Source Provider")]
    public class AsyncDiscordCompletionSourceProvider : IAsyncCompletionSourceProvider {
        public IAsyncCompletionSource GetOrCreate(ITextView textView) {
            return new AsyncDiscordCompletionSource();
        }
    }
}
