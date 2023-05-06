using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis.TestEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.ContextDetectorTests {
    internal class DiscordChannelContextTestEngine : CodeAnalysisTestEngine<DiscordChannelContext?> {
        public DiscordChannelContextTestEngine(string solutionPath, string projectName) : base(solutionPath, projectName) {
        }

        protected override async Task RunTestAsync(string key, DiscordChannelContext? value) {
            AsyncDiscordContextDetector detector = new AsyncDiscordContextDetector(base.SemanticModel);
            await detector.ExecuteAsync(SyntaxNode);
            DiscordChannelContext? foundContext = detector.ChannelType;

            if (value != foundContext)
                Errors.Add($"Teststring: {key} | Expected Context: {value} | Found Context: {foundContext}.");
        }
    }
}
