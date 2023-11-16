using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis.TestEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.ContextDetectorTests {
    internal class DiscordCompletionContextTestEngine : CodeAnalysisTestEngine<DiscordCompletionContext> {
        public DiscordCompletionContextTestEngine(string solutionPath, string projectName) : base(solutionPath, projectName) {
        }

        protected override async Task RunTestAsync(string key, DiscordCompletionContext value) {
            AsyncDiscordContextAnalyser detector = new AsyncDiscordContextAnalyser(base.SemanticModel);
            DiscordCompletionContext context = await detector.Run(SyntaxNode);

            if (value != context)
                Errors.Add($"Teststring: {key} | Expected Context: {value} | Found Context: {context}.");
        }
    }
}
