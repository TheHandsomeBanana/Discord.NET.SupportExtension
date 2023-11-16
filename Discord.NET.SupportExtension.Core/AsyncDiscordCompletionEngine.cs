using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Helper;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    internal class AsyncDiscordCompletionEngine : IAsyncDiscordCompletionEngine {
        public AsyncDiscordCompletionEngine() {
        }

        public async Task<IDiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token) {
            IEnumerable<IDiscordCompletionItem> completionItems = new List<IDiscordCompletionItem>();
            Project project = solution.Projects.FirstOrDefault(e => e.Documents.Any(f => f.FilePath == token.SyntaxTree.FilePath))
                ?? throw new InternalException($"Project from {token.SyntaxTree.FilePath} not found.");

            AsyncDiscordContextAnalyser contextDetector = new AsyncDiscordContextAnalyser(semanticModel);
            DiscordCompletionContext foundContext = await contextDetector.Run(token.Parent);

            AsyncDiscordAnalyser analyser = new AsyncDiscordAnalyser(semanticModel, semanticModel.SyntaxTree, solution, project, foundContext);
            DiscordServerCollection serverCollection = DIContainer.GetService<IServerCollectionHolder>().Get(project.Name);
            completionItems = (await analyser.Run(token.Parent))?.Select(e => CompletionHelper.ToCompletionItem(e, serverCollection)) ?? new IDiscordCompletionItem[0];

            return completionItems.ToArray();
        }
    }
}
