using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;

namespace Discord.NET.SupportExtension.Core.Helper {
    internal static class CompletionHelper {

        public static Interface.DiscordCompletionItem ToCompletionItem(DiscordEntity item, DiscordServerCollection serverCollection) {
            string suffix = item.Type == DiscordEntityType.Channel
                ? $"{(item as DiscordChannel)?.ChannelType.Value}"
                : $"{item.Type}";

            DiscordEntity parent = null;
            if (item.ParentId.HasValue) {
                parent = serverCollection.GetEntity(item.ParentId.Value);
                suffix += $" [{parent.Name}]";
            }

            ClassifiedTextElement serverTextElement = new ClassifiedTextElement(
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, "Server: "),
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, (parent != null ? parent.Name : item.Name) + " "),
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Number, parent != null ? parent.Id.ToString() : item.Id.ToString())
            );

            ContainerElement description;
            if (item.Type != DiscordEntityType.Server) {
                description = new ContainerElement(ContainerElementStyle.Stacked,
                    new ContainerElement(ContainerElementStyle.Wrapped,
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Type, $"{item.Type}: "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, item.Name + " "),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Number, $"{item.Id}")
                        )
                    ),
                    new ContainerElement(ContainerElementStyle.Wrapped, serverTextElement)
                );
            }
            else
                description = new ContainerElement(ContainerElementStyle.Wrapped, serverTextElement);

            return new DiscordCompletionItem {
                DisplayText = item.Name,
                InsertText = item.Id.ToString(),
                Suffix = suffix,
                Description = description
            };
        }
    }
}
