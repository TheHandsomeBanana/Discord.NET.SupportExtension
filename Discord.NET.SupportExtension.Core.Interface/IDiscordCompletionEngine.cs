using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public interface IDiscordCompletionEngine {
        Task<DiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token);
    }
}
