using Discord.NET.SupportExtension.ViewModels;
using HB.NETF.Unity;
using Unity;

namespace Discord.NET.SupportExtension {
    public class UnitySetup : IUnitySetup {
        public void Build(IUnityContainer container) {
            container.RegisterType<ConfigureServerImageViewModel>()
                .RegisterType<KeyEntryViewModel>();
        }
    }
}
