using Discord.NET.SupportExtension.Core.Interface;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.ContextDetector {
    internal class AsyncDiscordContextDetector {
        private SemanticModel SemanticModel;
        public AsyncDiscordContextDetector(SemanticModel semanticModel) {

        }

        public async Task<DiscordCompletionContext> ExecuteAsync(SyntaxNode syntaxNode) {

        }
    }
}
