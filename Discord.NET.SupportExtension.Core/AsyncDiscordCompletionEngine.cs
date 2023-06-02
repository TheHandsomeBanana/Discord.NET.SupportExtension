using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Helper;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.Exceptions;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    internal class AsyncDiscordCompletionEngine : IAsyncDiscordCompletionEngine {
        public async Task<IDiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token) {
            IEnumerable<IDiscordCompletionItem> completionItems;
            Project project = solution.Projects.FirstOrDefault(e => e.Documents.Any(f => f.FilePath == token.SyntaxTree.FilePath))
                ?? throw new InternalException($"Project from {token.SyntaxTree.FilePath} not found.");

            AsyncDiscordContextDetector contextDetector = new AsyncDiscordContextDetector(semanticModel);
            DiscordCompletionContext foundContext = await contextDetector.ExecuteAsync(token.Parent);

            AsyncDiscordContextAnalyser analyser = new AsyncDiscordContextAnalyser(semanticModel, semanticModel.SyntaxTree, solution, project, foundContext);
            completionItems = (await analyser.ExecuteAsync(token.Parent))?.Select(e => e.ToCompletionItem()) ?? new IDiscordCompletionItem[0];
            return completionItems.ToArray();
        }
    }
}
