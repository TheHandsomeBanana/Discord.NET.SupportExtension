using HB.NETF.Common.DependencyInjection;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Logging;
using HB.NETF.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.VisualStudio.Workspace;
using System.Windows.Forms;
using HB.NETF.VisualStudio.Commands;
using Discord.NET.SupportExtension.Helper;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using System.IO;

namespace Discord.NET.SupportExtension.Commands {
    internal sealed class LoadServerCollectionCommand : AsyncCommandBase {
        protected override Guid CommandSet => PackageGuids.CommandSet;
        protected override int CommandId => PackageIds.LoadServerCollectionCommand;

        private readonly IDiscordEntityService entityService;
        private readonly ILogger<LoadServerCollectionCommand> logger;

        internal LoadServerCollectionCommand(AsyncPackage package, IMenuCommandService commandService, Action<Exception> onException) : base(package, commandService, onException) {
            entityService = DIContainer.GetService<IDiscordEntityService>();
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<LoadServerCollectionCommand>();
        }

        public static LoadServerCollectionCommand Instance { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package, IMenuCommandService commandService, Action<Exception> onException) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            Instance = new LoadServerCollectionCommand(package, commandService, onException);
        }

        protected override async Task ExecuteAsync(object sender, EventArgs e) {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            string currentProjectName = SolutionHelper.GetCurrentProject().Name;
            string currentCachePath = DiscordSupportPackage.GetCachePath();

            await Package.JoinableTaskFactory.RunAsync(async () => {
                DiscordServerCollection serverCollection;
                try {
                    serverCollection = await entityService.ReadFromFile(currentCachePath);

                    IServerCollectionHolder serverCollectionHolder = DIContainer.GetService<IServerCollectionHolder>();
                    serverCollectionHolder.Hold(currentProjectName, serverCollection);

                    string message = InteractionMessages.ServerCollectionLoadedFor(currentProjectName);
                    UIHelper.ShowInfo(message);
                    logger.LogInformation(message);
                }
                catch(FileNotFoundException) {
                    logger.LogError(InteractionMessages.ImageNotFoundFor(currentProjectName));
                    UIHelper.ShowError(InteractionMessages.ImageNotFoundFor(currentProjectName));
                }
                catch (Exception ex) {
                    logger.LogError(ex.ToString());
                    UIHelper.ShowError(InteractionMessages.ServerCollectionNotLoadedFor(currentProjectName));
                }
            });
        }
    }
}
