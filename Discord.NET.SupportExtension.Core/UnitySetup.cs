using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Discord.NET.SupportExtension.Core {
    public class UnitySetup : IUnitySetup {
        public void Build(IUnityContainer container) {
            container.RegisterType<IDiscordCompletionEngine, DiscordCompletionEngine>()
                .RegisterType<IDiscordAnalyser, DiscordAnalyser>()
                .RegisterType<IDiscordContextAnalyser, DiscordContextAnalyser>()
                .RegisterType<IDiscordServerIdAnalyser, DiscordServerIdAnalyser>();
        }
    }
}
