using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.DataService;
using HB.NETF.Discord.NET.Toolkit.DataService.Models.Simplified;
using HB.NETF.Services.Security.Cryptography;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Storage;
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
                .AddSingleton<ISimplifiedMemoryService, MemoryService>()
                .AddSingleton<IGenCryptoService<SimplifiedDiscordDataModel>, AesCryptoService<SimplifiedDiscordDataModel>>()
                .AddSingleton<IDiscordDataServiceWrapper, DiscordDataServiceWrapper>();
        }
    }
}
