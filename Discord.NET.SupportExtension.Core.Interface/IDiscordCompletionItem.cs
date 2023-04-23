using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public interface IDiscordCompletionItem {
        string Id { get; }
        string Name { get; }
        DiscordCompletionContext CompletionContext { get; }
        
    }
}
