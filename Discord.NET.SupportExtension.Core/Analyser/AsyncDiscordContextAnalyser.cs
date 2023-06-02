using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.DataService;
using HB.NETF.Discord.NET.Toolkit.DataService.Models.Simplified;
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
    internal class AsyncDiscordContextAnalyser : IAsyncCodeAnalyser<DiscordItemModel[]> {
        private ILogger<AsyncDiscordContextAnalyser> logger;
        private IDiscordDataServiceWrapper discordDataService;

        private DiscordCompletionContext completionContext;
        private Solution solution;
        private Project project;
        private SemanticModel semanticModel;
        private SyntaxTree syntaxTree;
        

        public AsyncDiscordContextAnalyser(SemanticModel semanticModel, SyntaxTree syntaxTree, Solution solution, Project project, DiscordCompletionContext completionContext) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger<AsyncDiscordContextAnalyser>();
            discordDataService = DIContainer.GetService<IDiscordDataServiceWrapper>();

            this.completionContext = completionContext;
            this.solution = solution;
            this.project = project;
            this.semanticModel = semanticModel;
            this.syntaxTree = syntaxTree;
        }

        public async Task<DiscordItemModel[]> ExecuteAsync(SyntaxNode syntaxNode) {
            if (completionContext == DiscordCompletionContext.Server)
                return discordDataService.GetServers();

            IAsyncCodeAnalyser<IEnumerable<ulong>> serverIdAnalyser = new AsyncDiscordServerIdAnalyser(semanticModel, syntaxTree, solution, project);
            IEnumerable<ulong> serverIdList = await serverIdAnalyser.ExecuteAsync(syntaxNode);
            List<DiscordItemModel> foundItems = new List<DiscordItemModel>();
            switch(completionContext) {
                case DiscordCompletionContext.User:
                    foreach(ulong serverId in serverIdList)
                        foundItems.AddRange(discordDataService.GetUsers(serverId));
                    
                    return foundItems.ToArray();
                case DiscordCompletionContext.Role:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(discordDataService.GetRoles(serverId));

                    return foundItems.ToArray();
                case DiscordCompletionContext.Channel:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(discordDataService.GetChannels(serverId));

                    return foundItems.ToArray();
            }

            return null;
        }
    }
}
