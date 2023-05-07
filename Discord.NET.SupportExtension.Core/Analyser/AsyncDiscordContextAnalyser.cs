using HB.NETF.Code.Analysis;
using HB.NETF.Common.DependencyInjection;
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
        private const int RECURSION_LEVEL = 10;
        private Solution solution;
        private Project project;
        private IImmutableSet<Document> documents;
        private SemanticModel semanticModel;
        private SyntaxTree syntaxTree;
        private SyntaxNode currentNode;
        

        public AsyncDiscordContextAnalyser(SemanticModel semanticModel, SyntaxTree syntaxTree, Solution solution, Project project) {
            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
            logger = loggerFactory.CreateLogger<AsyncDiscordContextAnalyser>();


        }

        public async Task<DiscordItemModel[]> ExecuteAsync(SyntaxNode syntaxNode) {


            return null;
        }
    }
}
