using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.EntityService.Merged;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Helper {
    internal class CompletionHelper {
        private readonly IMergedDiscordEntityService entityService;
        public CompletionHelper() {
            entityService = DIContainer.GetService<IMergedDiscordEntityService>();
        }

        public IDiscordCompletionItem ToCompletionItem(DiscordEntity item) {

            string displayText = item.Type == DiscordEntityType.Channel 
                ? $"{item.Name} ({(item as DiscordChannel)?.ChannelType.Value})"
                : $"{item.Name} ({item.Type})";
            
            string insertText = item.Id.ToString();
            string suffix = insertText;

            if(item.ParentId.HasValue) {
                DiscordEntity parent = entityService.ServerCollection.GetEntity(item.ParentId.Value);
                suffix += $" -> {parent.Name}";
            }

            return new DiscordCompletionItem {
                DisplayText = displayText,
                InsertText = insertText,
                Suffix = suffix
            };
        }
    }
}
