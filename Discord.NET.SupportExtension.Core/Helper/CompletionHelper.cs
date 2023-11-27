using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Helper {
    internal static class CompletionHelper {

        public static Interface.DiscordCompletionItem ToCompletionItem(DiscordEntity item, DiscordServerCollection serverCollection) {

            string insertText = item.Id.ToString();

            string displayText = item.Name;

            string suffix = item.Type == DiscordEntityType.Channel
                ? $"{item.Id} ({(item as DiscordChannel)?.ChannelType.Value})"
                : $"{item.Id} ({item.Type})"; ;

            if(item.ParentId.HasValue) {
                DiscordEntity parent = serverCollection.GetEntity(item.ParentId.Value);
                suffix += $" [{parent.Name}]";
            }

            return new DiscordCompletionItem {
                DisplayText = displayText,
                InsertText = insertText,
                Suffix = suffix
            };
        }
    }
}
