using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface.Analyser {
    public interface ICodeAnalyser<TResult> {
        void Initialize(Solution solution, Project project, SemanticModel semanticModel);
        Task<TResult> Run(SyntaxNode syntaxNode);
    }
}
