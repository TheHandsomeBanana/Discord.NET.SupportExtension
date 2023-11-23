using HB.NETF.Code.Analysis.Analyser;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    public class DiscordAnalyserBase : AnalyserBase {
        public DiscordAnalyserBase() {
        }

        protected DiscordAnalyserBase(Solution solution, Project project, SemanticModel semanticModel, IImmutableSet<Document> documents) {
            base.Initialize(solution, project, documents, semanticModel);
        }

        protected bool HasSameSemantics(SyntaxNode node) => this.SyntaxTree.FilePath == node.SyntaxTree.FilePath;
        protected T GetNewAnalyser<T>(SyntaxNode node) where T : DiscordAnalyserBase, new() {
            SemanticModel sm = this.SemanticModel.Compilation.GetSemanticModel(node.SyntaxTree);
            T newAnalyser = new T();
            newAnalyser.Initialize(Solution, Project, Documents, sm);
            return newAnalyser;
        }
    }
}
