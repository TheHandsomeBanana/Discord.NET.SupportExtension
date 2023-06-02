using Discord.NET.SupportExtension.Core.Analyser;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.TestEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    internal class DiscordServerIdTestEngine : CodeAnalysisTestEngine<ulong[]> {
        public DiscordServerIdTestEngine(string solutionPath, string projectName) : base(solutionPath, projectName) {
        }

        protected override async Task RunTestAsync(string testString, ulong[] ids) {
            IAsyncCodeAnalyser<IEnumerable<ulong>> serverIdAnalyser = new AsyncDiscordServerIdAnalyser(SemanticModel, SyntaxTree, Solution, Project);
            IEnumerable<ulong> result = await serverIdAnalyser.ExecuteAsync(SyntaxNode);
        }
    }
}
