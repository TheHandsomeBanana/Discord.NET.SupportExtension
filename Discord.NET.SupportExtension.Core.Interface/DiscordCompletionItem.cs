using Microsoft.VisualStudio.Text.Adornments;

namespace Discord.NET.SupportExtension.Core.Interface {
    public class DiscordCompletionItem {
        public string DisplayText { get; set; }
        public string InsertText { get; set; }
        public string Suffix { get; set; }
        public ContainerElement Description { get; set; }
    }
}
