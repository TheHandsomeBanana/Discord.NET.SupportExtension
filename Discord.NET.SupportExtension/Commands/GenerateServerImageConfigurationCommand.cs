using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.VisualStudio.UI;
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
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateServerImageConfigurationCommand {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateServerImageConfigurationCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenerateServerImageConfigurationCommand(AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.CommandSet, PackageIds.GenerateServerImageConfigurationCommand);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
            this.streamHandler = DIContainer.GetService<IStreamHandler>();
            this.logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<GenerateServerImageConfigurationCommand>();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateServerImageConfigurationCommand Instance {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider {
            get {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in Command1's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateServerImageConfigurationCommand(package, commandService);
        }

        private IStreamHandler streamHandler;
        private ILogger<GenerateServerImageConfigurationCommand> logger;

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e) {
            ConfigureServerImageModel model = new ConfigureServerImageModel();

            try {
                if (File.Exists(ConfigHelper.GetConfigPath()))
                    model = streamHandler.ReadFromFile<ConfigureServerImageModel>(ConfigHelper.GetConfigPath());
                else
                    logger.LogInformation("No configuration file found. Window loaded with empty configuration.");
            }
            catch (InternalException ex) {
                logger.LogError(ex.ToString());
            }

            ConfigureServerImageView view = new ConfigureServerImageView() { DataContext = new ConfigureServerImageViewModel(model) };
            UIHelper.Show(view);
        }
    }
}
