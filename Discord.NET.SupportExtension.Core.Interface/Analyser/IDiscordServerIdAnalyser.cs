using System.Collections.Immutable;

namespace Discord.NET.SupportExtension.Core.Interface.Analyser {
    public interface IDiscordServerIdAnalyser : ICodeAnalyser<ImmutableArray<ulong>> {
    }
}
