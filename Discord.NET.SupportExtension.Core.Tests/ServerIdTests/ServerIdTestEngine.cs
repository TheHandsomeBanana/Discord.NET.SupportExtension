using Discord.NET.SupportExtension.Core.Analyser;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Tests.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    internal class ServerIdTestEngine : CodeAnalysisTestEngine<ulong[]> {

        protected override async Task RunTestAsync(string testString, ulong[] ids, string document) {
            DiscordServerIdAnalyser serverIdAnalyser = new DiscordServerIdAnalyser();
            serverIdAnalyser.Initialize(Solution, Project, SemanticModel);
            IEnumerable<ulong> result = await serverIdAnalyser.Run(SyntaxNode);
        }
    }
}
