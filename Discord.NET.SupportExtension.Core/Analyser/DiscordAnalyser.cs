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
using Unity;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class DiscordAnalyser : DiscordAnalyserBase, IDiscordAnalyser {
        [Dependency]
        public IServerCollectionHolder ServerHolder { get; set; }
        [Dependency]
        public IDiscordContextAnalyser ContextAnalyser { get; set; }
        [Dependency]
        public IDiscordServerIdAnalyser ServerIdAnalyser { get; set; }
        private DiscordServerCollection serverCollection;

        public override void Initialize(Solution solution, Project project, SemanticModel semanticModel) {
            base.Initialize(solution, project, semanticModel);
            serverCollection = ServerHolder.Get(project.Name);
        }

        public async Task<DiscordEntity[]> Run(SyntaxNode syntaxNode) {
            ContextAnalyser.Initialize(Solution, Project, SemanticModel);
            ServerIdAnalyser.Initialize(Solution, Project, SemanticModel);

            DiscordCompletionContext foundContext = await ContextAnalyser.Run(syntaxNode);
            if (foundContext == DiscordCompletionContext.Undefined)
                return Array.Empty<DiscordEntity>();

            if (foundContext.BaseContext == DiscordBaseCompletionContext.Server)
                return serverCollection.GetServers();

            ImmutableArray<ulong> serverIdList = await ServerIdAnalyser.Run(foundContext.ContextNode);
            if (serverIdList.IsEmpty || serverIdList.All(e => !serverCollection.Keys.Contains(e)))
                serverIdList = serverCollection.Keys.ToImmutableArray();

            List<DiscordEntity> foundItems = new List<DiscordEntity>();
            switch (foundContext.BaseContext) {
                case DiscordBaseCompletionContext.User:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetUsers(serverId));
                    break;
                case DiscordBaseCompletionContext.Role:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetRoles(serverId));
                    break;
                case DiscordBaseCompletionContext.Channel:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(serverCollection.GetChannels(serverId, MapChannelType(foundContext.ChannelContext)));
                    break;

            }

            return foundItems.ToArray();
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
                case DiscordChannelContext.Forum: return DiscordChannelType.Forum;
                case DiscordChannelContext.Private: return DiscordChannelType.Private;
            }

            return null;
        }
    }
}
