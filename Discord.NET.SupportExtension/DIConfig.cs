using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.NETF.Discord.NET.Toolkit.EntityService;
using HB.NETF.Services.Logging;
using Discord.NET.SupportExtension.Helper;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.VisualStudio.UI;

namespace Discord.NET.SupportExtension {
    internal class DIConfig : IDependencyConfig {
        public void Configure(DIBuilder builder) {
            builder.Services.AddSingleton<ILoggerFactory>(new LoggerFactory(b => b
                .AddTarget(DiscordSupportPackage.EventLogPath)
                .AddTarget(UIHelper.OutputWindowFunc)    
            ))
                .AddSingleton<IIdentifierFactory>(new IdentifierFactory());
        }
    }
}
