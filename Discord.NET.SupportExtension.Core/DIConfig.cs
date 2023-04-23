using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.DataService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    public class DIConfig : IDependencyConfig {
        public void Configure(DIBuilder builder) {
            builder.Services.AddSingleton<IAsyncDiscordCompletionEngine, AsyncDiscordCompletionEngine>()
                .AddSingleton<IDiscordCompletionItem, DiscordCompletionItem>()
                .AddSingleton<IDiscordDataService, DiscordDataService>();
        }
    }
}
