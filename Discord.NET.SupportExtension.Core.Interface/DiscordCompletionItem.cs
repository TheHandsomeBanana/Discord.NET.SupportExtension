using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public class DiscordCompletionItem {
        public string DisplayText { get; set; }
        public string InsertText { get; set; }
        public string Suffix { get; set; }
        public ContainerElement Description { get; set; }
    }
}
