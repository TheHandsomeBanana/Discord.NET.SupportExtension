using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Helper;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    internal class AsyncDiscordCompletionEngine : IAsyncDiscordCompletionEngine {
        private readonly CompletionHelper completionHelper;
        public AsyncDiscordCompletionEngine() {
            completionHelper = new CompletionHelper();
        }

        public async Task<IDiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token) {
            IEnumerable<IDiscordCompletionItem> completionItems = new List<IDiscordCompletionItem>();
            Project project = solution.Projects.FirstOrDefault(e => e.Documents.Any(f => f.FilePath == token.SyntaxTree.FilePath))
                ?? throw new InternalException($"Project from {token.SyntaxTree.FilePath} not found.");

            AsyncDiscordContextDetector contextDetector = new AsyncDiscordContextDetector(semanticModel);
            DiscordCompletionContext foundContext = await contextDetector.Run(token.Parent);

            AsyncDiscordContextAnalyser analyser = new AsyncDiscordContextAnalyser(semanticModel, semanticModel.SyntaxTree, solution, project, foundContext);
            completionItems = (await analyser.Run(token.Parent))?.Select(e => completionHelper.ToCompletionItem(e)) ?? new IDiscordCompletionItem[0];

            return completionItems.ToArray();
        }
    }
}
