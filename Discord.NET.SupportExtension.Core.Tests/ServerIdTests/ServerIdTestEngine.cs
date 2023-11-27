using Discord.NET.SupportExtension.Core.Analyser;
using HB.NETF.Code.Analysis;
using HB.NETF.Code.Analysis.Tests.Engine;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

            IEnumerable<ulong> result = await serverIdAnalyser.Run(FindAccordingParentNode());

            foreach(ulong id in ids) {
                if (!result.Contains(id))
                    Errors.Add($"[{document}] Teststring: {testString} | Testvalue {id} not found in ({string.Join(",", result)}).");
            }
        }

        private SyntaxNode FindAccordingParentNode() {
            SyntaxNode n = SyntaxNode;
            while (!n.IsKind(SyntaxKind.Block) && !n.IsKind(SyntaxKind.InvocationExpression)) {
                if (n is SimpleLambdaExpressionSyntax lambda)
                    return n.DescendantNodes().OfType<IdentifierNameSyntax>().First(); // return lambda parameter identifier

                n = n.Parent;
            }

            if(n is InvocationExpressionSyntax)
                return n;
            
            return SyntaxNode;
        }
    }
}
