using HB.NETF.Code.Analysis.TestEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    internal class DiscordServerDetailTestEngine : CodeAnalysisTestEngine<ulong[]> {
        public DiscordServerDetailTestEngine(string solutionPath, string projectName) : base(solutionPath, projectName) {
        }

        protected override async Task RunTestAsync(string testString, ulong[] ids) {
            
        }
    }
}
