﻿using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.NETF.Services.Logging;
using Discord.NET.SupportExtension.Helper;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.VisualStudio.UI;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using HB.NETF.Services.Serialization;
using System.Data;
using HB.NETF.Services.Security.DataProtection;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;

namespace Discord.NET.SupportExtension {
    internal class DIConfig : IDependencyConfig {
        public void Configure(DIBuilder builder) {
            builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory(b => b
                .AddTarget(DiscordSupportPackage.EventLogPath)
                .AddTarget(UIHelper.OutputWindowFunc)
            ))
                .AddSingleton<IIdentifierFactory, IdentifierFactory>()
                .AddSingleton<IServerCollectionHolder, ServerCollectionHolder>()
                .AddTransient<IDiscordEntityService, DiscordRestEntityService>()
                .AddTransient<IStreamHandler, StreamHandler>()
                .AddTransient<IAsyncStreamHandler, AsyncStreamHandler>()
                .AddTransient<IDiscordTokenService, DiscordTokenService>()
                .AddTransient<IAesCryptoService, AesCryptoService>()
                .AddTransient<IDataProtectionService, DataProtectionService>()
                .AddTransient<ISerializerService, SerializerService>();
        }
    }
}
