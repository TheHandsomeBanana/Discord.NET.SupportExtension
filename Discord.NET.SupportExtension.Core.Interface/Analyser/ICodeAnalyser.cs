using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface.Analyser {
    public interface ICodeAnalyser<TResult> {
        void Initialize(Solution solution, Project project, SemanticModel semanticModel);
        Task<TResult> Run(SyntaxNode syntaxNode);
    }
}
