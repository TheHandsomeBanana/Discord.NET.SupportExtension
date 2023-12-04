using Microsoft.CodeAnalysis;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public interface IDiscordCompletionEngine {
        Task<DiscordCompletionItem[]> ProcessCompletionAsync(Solution solution, SemanticModel semanticModel, SyntaxToken token);
    }
}
