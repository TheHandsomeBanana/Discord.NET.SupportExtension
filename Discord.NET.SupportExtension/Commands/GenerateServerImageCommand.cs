﻿using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Keys;
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
using System.Linq;
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

        private readonly ILogger<GenerateServerImageCommand> logger;
        private void Execute(object sender, EventArgs e) {
            ThreadHelper.ThrowIfNotOnUIThread();

            package.JoinableTaskFactory.Run(async () => {
                IDiscordTokenService tokenService = DIContainer.GetService<IDiscordTokenService>();

                string token;

                try {
                    ConfigureServerImageModel model = new ConfigureServerImageModel();
                    if (File.Exists(ConfigHelper.GetConfigPath()))
                        model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(ConfigHelper.GetConfigPath());

                    token = GenerateHelper.GetToken(tokenService, model, logger);

                    if (string.IsNullOrWhiteSpace(token)) {
                        logger.LogInformation("Get token failed, server image generation aborted.");
                        return;
                    }

                    IDiscordEntityService entityService = DIContainer.GetService<IDiscordEntityService>();
                    entityService.OnTimeout += OnTimeout;

                    GenerateHelper.ManipulateEntityServiceDataEncrypt(entityService, model, logger, out bool cancel);
                    if (cancel) {
                        logger.LogInformation("Server image generation cancelled.");
                        return;
                    }

                    await entityService.Connect(token);
                    if(entityService.Ready) {
                        await entityService.LoadEntities();
                        await entityService.SaveToFile(DiscordSupportPackage.GetCachePath());
                        logger.LogInformation("Server image successfully generated.");
                        UIHelper.ShowInfo("Server Image generated.", "Success");

                        model.LatestRun = DateTime.Now;
                        await streamHandler.WriteToFileAsync(ConfigHelper.GetConfigPath(), model);
                    }

                    entityService.Dispose();
                }
                catch (InternalException ex) {
                    this.logger.LogError(ex.ToString());
                    UIHelper.ShowError("Server image generation failed.", "Error");
                    return;
                }
            });
        }

        private void OnTimeout() {
            logger.LogError("Could not generate image, connection timed out");
            UIHelper.ShowError("Could not generate image.", "Failure");
        }
    }
}
