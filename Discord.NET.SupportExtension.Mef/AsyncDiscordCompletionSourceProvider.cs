using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.MEF {
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [ContentType("CSharp")]
    [Name("Discord Provider")]
    internal class AsyncDiscordCompletionSourceProvider : IAsyncCompletionSourceProvider {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public IAsyncCompletionSource GetOrCreate(ITextView textView) {
            return new AsyncDiscordCompletionSource();
        }
    }
}
