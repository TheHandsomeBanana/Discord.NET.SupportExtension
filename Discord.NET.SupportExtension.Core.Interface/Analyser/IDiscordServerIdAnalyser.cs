using HB.NETF.Code.Analysis.Interface;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface.Analyser {
    public interface IDiscordServerIdAnalyser : ICodeAnalyser<ImmutableArray<ulong>> {
    }
}
