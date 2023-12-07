using Discord.NET.SupportExtension.Core.Helper;
using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace Discord.NET.SupportExtension.Core {
    internal class DiscordCompletionEngine : IDiscordCompletionEngine {
        [Dependency]
        public IDiscordAnalyser DiscordAnalyser { get; set; }
        [Dependency]
        public IServerCollectionHolder ServerHolder { get; set; }
        public DiscordCompletionEngine() {
        }

        public async Task<Interface.DiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token) {
            DiscordCompletionItem[] completionItems = Array.Empty<DiscordCompletionItem>();
            Project project = solution.Projects.FirstOrDefault(e => e.Documents.Any(f => f.FilePath == token.SyntaxTree.FilePath))
                ?? throw new InternalException($"Project from {token.SyntaxTree.FilePath} not found.");

            DiscordServerCollection serverCollection = ServerHolder.Get(project.Name);
            if (serverCollection.Keys.Count == 0)
                return completionItems;

            DiscordAnalyser.Initialize(solution, project, semanticModel);
            completionItems = (await DiscordAnalyser.Run(token.Parent)).Select(e => CompletionHelper.ToCompletionItem(e, serverCollection)).ToArray();

            return completionItems;
        }
    }
}
