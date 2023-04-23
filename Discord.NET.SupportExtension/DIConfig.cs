using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography;
using HB.NETF.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension {
    internal class DIConfig : IDependencyConfig {
        public void Configure(DIBuilder builder) {
            builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory(b => b.AddTarget(DiscordSupportPackage.EventLogPath)));
            builder.Services.AddSingleton<ISimplifiedMemoryService, MemoryService>();
            builder.Services.AddSingleton<ICryptoService, AesCryptoService>();
        }
    }
}
