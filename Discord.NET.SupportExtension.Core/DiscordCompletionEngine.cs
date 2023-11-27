using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Helper;
using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    internal class DiscordCompletionEngine : IDiscordCompletionEngine {
        private readonly IDiscordAnalyser discordAnalyser;
        private readonly IServerCollectionHolder serverHolder;
        public DiscordCompletionEngine(IDiscordAnalyser discordAnalyser, IServerCollectionHolder serverHolder) {
            this.discordAnalyser = discordAnalyser;
            this.serverHolder = serverHolder;
        }

        public async Task<Interface.DiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token) {
            DiscordCompletionItem[] completionItems = Array.Empty<DiscordCompletionItem>();
            Project project = solution.Projects.FirstOrDefault(e => e.Documents.Any(f => f.FilePath == token.SyntaxTree.FilePath))
                ?? throw new InternalException($"Project from {token.SyntaxTree.FilePath} not found.");

            discordAnalyser.Initialize(solution, project, semanticModel);
            completionItems = (await discordAnalyser.Run(token.Parent)).Select(e => CompletionHelper.ToCompletionItem(e, serverHolder.Get(project.Name))).ToArray();

            return completionItems;
        }
    }
}
