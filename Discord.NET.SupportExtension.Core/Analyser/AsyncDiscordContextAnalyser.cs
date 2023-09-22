using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
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
    internal class AsyncDiscordContextAnalyser : IAsyncCodeAnalyser<DiscordEntityModel[]> {
        private ILogger<AsyncDiscordContextAnalyser> logger;
        private IDiscordEntityServiceHandler entityService;

        private DiscordCompletionContext completionContext;
        private Solution solution;
        private Project project;
        private SemanticModel semanticModel;
        private SyntaxTree syntaxTree;
        
        public AsyncDiscordContextAnalyser(SemanticModel semanticModel, SyntaxTree syntaxTree, Solution solution, Project project, DiscordCompletionContext completionContext) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            logger = loggerFactory.GetOrCreateLogger<AsyncDiscordContextAnalyser>();
            entityService = DIContainer.GetService<IDiscordEntityServiceHandler>();

            this.completionContext = completionContext;
            this.solution = solution;
            this.project = project;
            this.semanticModel = semanticModel;
            this.syntaxTree = syntaxTree;
        }

        public async Task<DiscordEntityModel[]> ExecuteAsync(SyntaxNode syntaxNode) {
            if (completionContext == DiscordCompletionContext.Server)
                return entityService.GetServers();

            IAsyncCodeAnalyser<IEnumerable<ulong>> serverIdAnalyser = new AsyncDiscordServerIdAnalyser(semanticModel, syntaxTree, solution, project);
            IEnumerable<ulong> serverIdList = await serverIdAnalyser.ExecuteAsync(syntaxNode);
            List<DiscordEntityModel> foundItems = new List<DiscordEntityModel>();
            switch(completionContext) {
                case DiscordCompletionContext.User:
                    foreach(ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.GetUsers(serverId));
                    
                    return foundItems.ToArray();
                case DiscordCompletionContext.Role:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.GetRoles(serverId));

                    return foundItems.ToArray();
                case DiscordCompletionContext.Channel:
                    foreach (ulong serverId in serverIdList)
                        foundItems.AddRange(entityService.GetChannels(serverId));

                    return foundItems.ToArray();
            }

            return null;
        }
    }
}
