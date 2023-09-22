using Discord.NET.SupportExtension.Core.Completions;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit.EntityService;
using HB.NETF.Discord.NET.Toolkit.EntityService.Cached.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
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
            builder.Services.AddSingleton<IAsyncDiscordCompletionEngine, AsyncDiscordCompletionEngine>()
                .AddTransient<IStreamHandler, StreamHandler>()
                .AddTransient<IAsyncStreamHandler, AsyncStreamHandler>()
                .AddSingleton<ICryptoService, AesCryptoService>()
                .AddSingleton<ICachedDiscordEntityServiceHandler, CachedDiscordEntityServiceHandler>();
        }
    }
}
