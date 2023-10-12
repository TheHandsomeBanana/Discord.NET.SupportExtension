using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Merged;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class AsyncDiscordContextAnalyser : ISnapshotAnalyser<DiscordEntity[]> {
        private readonly ILogger<AsyncDiscordContextAnalyser> logger;
        private readonly IMergedDiscordEntityService entityService;

        private readonly DiscordCompletionContext completionContext;
        private readonly Solution solution;
        private readonly Project project;
        private readonly SemanticModel semanticModel;
        private readonly SyntaxTree syntaxTree;

        public AsyncDiscordContextAnalyser(SemanticModel semanticModel, SyntaxTree syntaxTree, Solution solution, Project project, DiscordCompletionContext completionContext) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            logger = loggerFactory.GetOrCreateLogger<AsyncDiscordContextAnalyser>();
            entityService = DIContainer.GetService<IMergedDiscordEntityService>();

            this.completionContext = completionContext;
            this.solution = solution;
            this.project = project;
            this.semanticModel = semanticModel;
            this.syntaxTree = syntaxTree;
        }

        public async Task<DiscordEntity[]> ExecuteAsync(SyntaxNode syntaxNode) {
            if (completionContext.BaseContext == DiscordBaseCompletionContext.Server)
                return entityService.ServerCollection.GetServers();

            ISnapshotAnalyser<IEnumerable<ulong>> serverIdAnalyser = new AsyncDiscordServerIdAnalyser(semanticModel, syntaxTree, solution, project);
            IEnumerable<ulong> serverIdList = await serverIdAnalyser.ExecuteAsync(syntaxNode);
            List<DiscordEntity> foundItems = new List<DiscordEntity>();
            switch (completionContext.BaseContext) {
                case DiscordBaseCompletionContext.User:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.ServerCollection.GetUsers(serverId));

                    return foundItems.ToArray();
                case DiscordBaseCompletionContext.Role:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.ServerCollection.GetRoles(serverId));

                    return foundItems.ToArray();
                case DiscordBaseCompletionContext.Channel:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.ServerCollection.GetChannels(serverId, MapChannelType(completionContext.ChannelContext)));
                    

                    return foundItems.ToArray();
            }

            return null;
        }

        private DiscordChannelType? MapChannelType(DiscordChannelContext? context) {
            switch (context) {
                case DiscordChannelContext.Text: return DiscordChannelType.Text;
                case DiscordChannelContext.Voice: return DiscordChannelType.Voice;
                case DiscordChannelContext.Category: return DiscordChannelType.Category;
                case DiscordChannelContext.Guild: return DiscordChannelType.Guild;
                case DiscordChannelContext.Group: return DiscordChannelType.Group;
                case DiscordChannelContext.Stage: return DiscordChannelType.Stage;
                case DiscordChannelContext.Thread: return DiscordChannelType.Thread;
                case DiscordChannelContext.DM: return DiscordChannelType.DM;
            }

            return null;
        }
    }
}
