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

namespace Discord.NET.SupportExtension.Commands {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class LoadServerCollectionCommand : AsyncCommandBase {


        protected override Guid CommandSet => PackageGuids.CommandSet;
        protected override int CommandId => PackageIds.LoadServerCollectionCommand;

        private readonly IDiscordEntityService entityService;
        private readonly ILogger<GenerateServerImageCommand> logger;

        internal LoadServerCollectionCommand(AsyncPackage package, OleMenuCommandService commandService, Action<Exception> onException) : base(package, commandService, onException) {
            entityService = DIContainer.GetService<IDiscordEntityService>();
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<GenerateServerImageCommand>();
        }

        public static LoadServerCollectionCommand Instance { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package, Action<Exception> onException) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new LoadServerCollectionCommand(package, commandService, onException);
        }

        protected override async Task ExecuteAsync(object sender, EventArgs e) {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            string currentProjectName = SolutionHelper.GetCurrentProject().Name;
            string currentCachePath = DiscordSupportPackage.GetCachePath();

            await Package.JoinableTaskFactory.RunAsync(async () => {
                bool success = false;

                try {
                    success = await entityService.ReadFromFile(currentCachePath);
                }
                catch (Exception ex) {
                    logger.LogError(ex.ToString());
                }

                if (success) {
                    string message = InteractionHelper.Messages.ServerCollectionLoadedFor(currentProjectName);
                    UIHelper.ShowInfo(message, "Success");
                    logger.LogInformation(message);
                }
                else {
                    string message = InteractionHelper.Messages.ServerCollectionNotLoadedFor(currentProjectName);
                    UIHelper.ShowError(message, "Failure");
                    logger.LogError(message);
                }
            });
        }
    }
}
