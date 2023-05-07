using HB.NETF.Code.Analysis;
using HB.NETF.Discord.NET.Toolkit.DataService.Models.Simplified;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Analyser {
    internal class AsyncDiscordContextAnalyser : IAsyncCodeAnalyser<DiscordItemModel[]> {

        public async Task<DiscordItemModel[]> ExecuteAsync(SyntaxNode syntaxNode) {
            return null;
        }
    }
}
