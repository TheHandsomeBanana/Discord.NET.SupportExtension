using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.VisualStudio.Commands;
using HB.NETF.VisualStudio.UI;
using HB.NETF.VisualStudio.Workspace;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Discord.NET.SupportExtension.Commands {
    internal sealed class GenerateServerImageConfigurationCommand : AsyncCommandBase {
        protected override Guid CommandSet => PackageGuids.CommandSet;
        protected override int CommandId => PackageIds.GenerateServerImageConfigurationCommand;

        private static IAsyncStreamHandler streamHandler;
        private readonly ILogger<GenerateServerImageConfigurationCommand> logger;

        internal GenerateServerImageConfigurationCommand(AsyncPackage package, OleMenuCommandService commandService, Action<Exception> onException) : base(package, commandService, onException) {
            streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<GenerateServerImageConfigurationCommand>();
        }

        public static GenerateServerImageConfigurationCommand Instance { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package, Action<Exception> onException) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateServerImageConfigurationCommand(package, commandService, onException);
        }

        protected override async Task ExecuteAsync(object sender, EventArgs e) {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            string currentConfigPath = ConfigHelper.GetConfigPath();
            string currentProjectName = SolutionHelper.GetCurrentProject().Name;


            await Package.JoinableTaskFactory.RunAsync(async () => {
                ConfigureServerImageModel model = new ConfigureServerImageModel();

                try {
                    if (File.Exists(currentConfigPath)) {
                        model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(currentConfigPath);
                        logger.LogInformation(InteractionMessages.ConfigurationFoundFor(currentProjectName));
                    }
                    else
                        logger.LogInformation(InteractionMessages.ConfigurationNotFoundFor(currentProjectName) + ", " + InteractionMessages.WindowWithEmptyConfiguration);
                }
                catch (InternalException ex) {
                    logger.LogError(ex.ToString());
                }

                ConfigureServerImageView view = new ConfigureServerImageView() { DataContext = new ConfigureServerImageViewModel(model) };
                UIHelper.Show(view);
            });
           
        }
    }
}
