using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using Humanizer.DateTimeHumanizeStrategy;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordAnalyser : DiscordAnalyserBase, IDiscordAnalyser {
        private readonly ILogger<DiscordAnalyser> logger;
        private readonly IServerCollectionHolder serverHolder;
        private readonly IDiscordContextAnalyser contextAnalyser;
        private readonly IDiscordServerIdAnalyser serverIdAnalyser;
        private DiscordServerCollection serverCollection;

        public DiscordAnalyser(ILoggerFactory loggerFactory, IServerCollectionHolder holder, IDiscordContextAnalyser contextAnalyser, IDiscordServerIdAnalyser serverIdAnalyser) {
            logger = loggerFactory.GetOrCreateLogger<DiscordAnalyser>();
            serverHolder = holder; 
            this.contextAnalyser = contextAnalyser;
            this.serverIdAnalyser = serverIdAnalyser;
        }

        public override void Initialize(Solution solution, Project project, SemanticModel semanticModel) {
            base.Initialize(solution, project, semanticModel);
            serverCollection = serverHolder.Get(project.Name);
        }

        public async Task<DiscordEntity[]> Run(SyntaxNode syntaxNode) {
            contextAnalyser.Initialize(Solution, Project, SemanticModel);

            DiscordCompletionContext foundContext = await contextAnalyser.Run(syntaxNode);
            if (foundContext == DiscordCompletionContext.Undefined)
                return Array.Empty<DiscordEntity>();

            if (foundContext.BaseContext == DiscordBaseCompletionContext.Server)
                return serverCollection.GetServers();

            ImmutableArray<ulong> serverIdList = await serverIdAnalyser.Run(syntaxNode);
            List<DiscordEntity> foundItems = new List<DiscordEntity>();
            switch (foundContext.BaseContext) {
                case DiscordBaseCompletionContext.User:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetUsers(serverId));

                    return foundItems.ToArray();
                case DiscordBaseCompletionContext.Role:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetRoles(serverId));

                    return foundItems.ToArray();
                case DiscordBaseCompletionContext.Channel:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetChannels(serverId, MapChannelType(foundContext.ChannelContext)));
                    

                    return foundItems.ToArray();
            }

            return Array.Empty<DiscordEntity>(); ;
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
