using Discord.NET.SupportExtension.Core.ContextDetector;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis.Tests.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.ContextTests {
    internal class ContextTestEngine : CodeAnalysisTestEngine<DiscordCompletionContext> {
        protected override async Task RunTestAsync(string key, DiscordCompletionContext value, string document) {
            AsyncDiscordContextAnalyser contextAnalyser = new AsyncDiscordContextAnalyser(base.SemanticModel);
            DiscordCompletionContext context = await contextAnalyser.Run(SyntaxNode);

            if (value != context)
                Errors.Add($"[{document}] Teststring: {key} | Expected Context: {value} | Found Context: {context}.");
        }
    }
}
