using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography;
using HB.NETF.Services.Security.DataProtection;
using HB.NETF.Services.Serialization;
using HB.NETF.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using HB.NETF.VisualStudio.UI;
using Unity.Lifetime;
using Discord.NET.SupportExtension.ViewModels;

namespace Discord.NET.SupportExtension {
    public class UnitySetup : IUnitySetup {
        public void Build(IUnityContainer container) {
            container.RegisterType<ConfigureServerImageViewModel>()
                .RegisterType<KeyEntryViewModel>();
        }
    }
}
