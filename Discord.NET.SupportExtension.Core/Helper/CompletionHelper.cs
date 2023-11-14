using Discord.NET.SupportExtension.Core.Completions;
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

        public static IDiscordCompletionItem ToCompletionItem(DiscordEntity item, DiscordServerCollection serverCollection) {

            string insertText = item.Id.ToString();

            string suffix = item.Type == DiscordEntityType.Channel
                ? $"{item.Name} ({(item as DiscordChannel)?.ChannelType.Value})"
                : $"{item.Name} ({item.Type})"; ;

            if(item.ParentId.HasValue) {
                DiscordEntity parent = serverCollection.GetEntity(item.ParentId.Value);
                suffix += $" -> {parent.Name}";
            }

            return new DiscordCompletionItem {
                DisplayText = insertText,
                InsertText = insertText,
                Suffix = suffix
            };
        }
    }
}
