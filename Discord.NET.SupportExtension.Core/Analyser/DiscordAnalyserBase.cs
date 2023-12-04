using HB.NETF.Code.Analysis.Analyser;
using HB.NETF.Code.Analysis.Resolver;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    public abstract class DiscordAnalyserBase {
        protected Solution Solution { get; private set; }
        protected Project Project { get; private set; }
        protected IImmutableSet<Document> Documents { get; private set; }
        protected SemanticModel SemanticModel { get; private set; }
        protected SyntaxTree SyntaxTree { get; private set; }

        public DiscordAnalyserBase() {
        }

        public virtual void Initialize(Solution solution, Project project, SemanticModel semanticModel) {
            this.Initialize(solution, project, project.Documents.ToImmutableHashSet(), semanticModel);
        }

        public void Initialize(Solution solution, Project project, IImmutableSet<Document> documents, SemanticModel semanticModel) {
            this.Solution = solution;
            this.Project = project;
            this.Documents = documents;
            this.SemanticModel = semanticModel;
            this.SyntaxTree = semanticModel.SyntaxTree;
        }


        protected bool HasSameSemantics(SyntaxNode node) => this.SyntaxTree.FilePath == node.SyntaxTree.FilePath;
        protected T GetNewAnalyser<T>(SyntaxNode node) where T : DiscordAnalyserBase, new() {
            SemanticModel sm = this.SemanticModel.Compilation.GetSemanticModel(node.SyntaxTree);
            T newAnalyser = new T();
            newAnalyser.Initialize(Solution, Project, Documents, sm);
            return newAnalyser;
        }

        protected async Task<IEnumerable<SyntaxNode>> GetReferences(ISymbol symbol) {
            IEnumerable<Location> locations = await LocationResolver.FindReferenceLocations(symbol, Solution, Documents);
            return LocationResolver.GetNodesFromLocations(locations);
        }

        protected async Task<IEnumerable<SyntaxNode>> GetCallers(ISymbol symbol) {
            IEnumerable<Location> locations = await LocationResolver.FindCallerLocations(symbol, Solution, Documents);
            return LocationResolver.GetNodesFromLocations(locations);
        }
    }
}
