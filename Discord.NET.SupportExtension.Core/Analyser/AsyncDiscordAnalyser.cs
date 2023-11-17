using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
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
    internal class AsyncDiscordAnalyser : DiscordAnalyserBase, ICodeAnalyser<DiscordEntity[]> {
        private readonly ILogger<AsyncDiscordAnalyser> logger;
        private readonly DiscordServerCollection serverCollection;

        public AsyncDiscordAnalyser(Solution solution, Project project, SemanticModel semanticModel) : base(solution, project, semanticModel) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            logger = loggerFactory.GetOrCreateLogger<AsyncDiscordAnalyser>();
            serverCollection = DIContainer.GetService<IServerCollectionHolder>().Get(project.Name);
        }

        public async Task<DiscordEntity[]> Run(SyntaxNode syntaxNode) {
            AsyncDiscordContextAnalyser contextDetector = new AsyncDiscordContextAnalyser(Solution, Project, SemanticModel);
            DiscordCompletionContext foundContext = await contextDetector.Run(syntaxNode);
            if (foundContext == DiscordCompletionContext.Undefined)
                return Array.Empty<DiscordEntity>();

            if (foundContext.BaseContext == DiscordBaseCompletionContext.Server)
                return serverCollection.GetServers();

            ICodeAnalyser<IEnumerable<ulong>> serverIdAnalyser = new AsyncDiscordServerIdAnalyser(Solution, Project, SemanticModel);
            IEnumerable<ulong> serverIdList = await serverIdAnalyser.Run(syntaxNode);
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


        async Task<object> ICodeAnalyser.Run(SyntaxNode syntaxNode) => await Run(syntaxNode);

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
