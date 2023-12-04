using Discord.NET.SupportExtension.Core.Analyser;
using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Interface.Analyser;
using HB.NETF.Unity;
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
