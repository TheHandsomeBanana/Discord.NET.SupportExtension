using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Server"),
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, parent != null ? parent.Id.ToString() : item.Id.ToString()),
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, parent != null ? parent.Name : item.Name)
            );

            ContainerElement description;
            if (item.Type != DiscordEntityType.Server) {
                description = new ContainerElement(ContainerElementStyle.Stacked,
                    new ContainerElement(ContainerElementStyle.Wrapped, serverTextElement),
                    new ContainerElement(ContainerElementStyle.Wrapped,
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, item.Type.ToString()),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, item.Id.ToString()),
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Text, item.Name)
                        )
                    )
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
