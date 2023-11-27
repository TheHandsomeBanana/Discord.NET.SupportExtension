using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Security.Cryptography;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography.Keys;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    public class DIConfig : IDependencyConfig {
        public void Configure(DIBuilder builder) {
            builder.Services.AddSingleton<IDiscordCompletionEngine, DiscordCompletionEngine>()
                .AddTransient<IDiscordAnalyser, DiscordAnalyser>()
                .AddTransient<IDiscordContextAnalyser, DiscordContextAnalyser>()
                .AddTransient<IDiscordServerIdAnalyser, DiscordServerIdAnalyser>();
        }
    }
}
