using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public interface IDiscordCompletionItem {
        string DisplayText { get; }
        string InsertText { get; }
        string Suffix { get; }
        

    }
}
