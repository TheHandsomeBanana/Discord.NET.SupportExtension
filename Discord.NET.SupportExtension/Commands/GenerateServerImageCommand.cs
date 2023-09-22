using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.EntityService;
using HB.NETF.Discord.NET.Toolkit.EntityService.Cached.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Settings;
using HB.NETF.VisualStudio.UI;
using HB.NETF.VisualStudio.Workspace;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using Task = System.Threading.Tasks.Task;

namespace Discord.NET.SupportExtension.Commands {
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class GenerateServerImageCommand {
        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private static IAsyncStreamHandler streamHandler;


        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateServerImageCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private GenerateServerImageCommand(AsyncPackage package, OleMenuCommandService commandService) {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.CommandSet, PackageIds.GenerateServerImageCommand);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<GenerateServerImageCommand>();
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static GenerateServerImageCommand Instance { get; private set; }



        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider { get => this.package; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package) {
            // Switch to the main thread - the call to AddCommand in GenerateServerImageCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateServerImageCommand(package, commandService);

        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>

        private ICachedDiscordEntityServiceHandler entityServiceHandler;
        private ILogger<GenerateServerImageCommand> logger;
        private void Execute(object sender, EventArgs e) {
            ThreadHelper.ThrowIfNotOnUIThread();


            package.JoinableTaskFactory.Run(async () => {

                List<TokenModel> tokens = new List<TokenModel>();
                try {
                    ConfigureServerImageModel model = new ConfigureServerImageModel();
                    if (File.Exists(ConfigHelper.GetConfigPath()))
                        model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(ConfigHelper.GetConfigPath());

                    if (model.SaveTokens) {
                        switch (model.TokenEncryptionMode) {
                            case EncryptionMode.AES:
                                HandleKeyExtractorUI(model.TokenKeyIdentifier, "Token", out bool cancel);
                                if (cancel)
                                    return;
                                break;
                            case EncryptionMode.WindowsDataProtectionAPI:
                                break;
                        }
                    }
                    else {
                        TokenEntryModel tokenEntryModel = new TokenEntryModel();
                        TokenEntryView view = new TokenEntryView { DataContext = new TokenEntryViewModel(tokenEntryModel) };
                        UIHelper.Show(view);
                    }

                    if (model.EncryptData) {
                        switch (model.DataEncryptionMode) {
                            case EncryptionMode.AES:
                                HandleKeyExtractorUI(model.DataKeyIdentifier, "Data", out bool cancel);
                                if (cancel)
                                    return;
                                break;
                            case EncryptionMode.WindowsDataProtectionAPI:
                                break;
                        }
                    }






                    entityServiceHandler = DIContainer.GetService<ICachedDiscordEntityServiceHandler>();
                    entityServiceHandler.Init();

                    await entityServiceHandler.Refresh();
                    entityServiceHandler.Dispose();
                }
                catch (InternalException ex) {
                    this.logger.LogError(ex.ToString());
                    UIHelper.ShowError("Image generation failed.", "Error");
                    return;
                }
            });

            UIHelper.ShowInfo("Server Image generated.", "Success");
        }

        private void HandleKeyExtractorUI(Guid? id, string name, out bool cancel) {
            cancel = false;
            KeyEntryModel keyEntry = new KeyEntryModel(name);
            KeyEntryView view = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            logger.LogInformation($"Requesting aes key for {name}.");
            UIHelper.Show(view);
            if (keyEntry.Key == null) {
                logger.LogInformation("Request cancelled.");
                cancel = true;
                return;
            }

            if (!keyEntry.Key.Identify(id.GetValueOrDefault())) {
                string error = $"Wrong key for {name} provided.";
                UIHelper.ShowError(error, "Wrong key");
                this.logger.LogError(error);
                cancel = true;
            }
        }
    }
}
