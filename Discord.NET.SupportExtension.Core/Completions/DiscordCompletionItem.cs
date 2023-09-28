using Discord.NET.SupportExtension.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Completions {
    internal class DiscordCompletionItem : IDiscordCompletionItem {
        public string DisplayText { get; set; }
        public string InsertText { get; set; }
        public string Suffix { get; set; }

    }
}
