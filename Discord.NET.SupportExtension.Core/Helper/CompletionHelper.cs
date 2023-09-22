using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Helper {
    internal static class CompletionHelper {
        public static IDiscordCompletionItem ToCompletionItem(this DiscordEntityModel item) {
            return new DiscordCompletionItem {
                Name = item.Name,
                CompletionContext = item.ItemModelType.ToCompletionContext(),
                Id = item.Id.ToString()
            }; 
        }

        private static DiscordCompletionContext ToCompletionContext(this DiscordItemModelType modelType) {
            switch (modelType) {
                case DiscordItemModelType.Server:
                    return DiscordCompletionContext.Server;
                case DiscordItemModelType.User:
                    return DiscordCompletionContext.User;
                case DiscordItemModelType.Role:
                    return DiscordCompletionContext.Role;
                case DiscordItemModelType.Channel:
                    return DiscordCompletionContext.Channel;
            }

            return DiscordCompletionContext.Undefined;
        }
    }
}
