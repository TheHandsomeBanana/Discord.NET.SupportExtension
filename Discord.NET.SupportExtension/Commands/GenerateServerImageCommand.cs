using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.ViewModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.EntityService;
using HB.NETF.Discord.NET.Toolkit.EntityService.Cached.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.TokenService;
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

        private ICachedDiscordEntityServiceHandler entityServiceHandler;
        private IDiscordTokenService tokenService;
        private ILogger<GenerateServerImageCommand> logger;
        private void Execute(object sender, EventArgs e) {
            ThreadHelper.ThrowIfNotOnUIThread();
            tokenService = DIContainer.GetService<IDiscordTokenService>();

            package.JoinableTaskFactory.Run(async () => {

                TokenModel[] tokens;

                try {
                    ConfigureServerImageModel model = new ConfigureServerImageModel();
                    if (File.Exists(ConfigHelper.GetConfigPath()))
                        model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(ConfigHelper.GetConfigPath());

                    if (model.SaveTokens) {
                        switch (model.TokenEncryptionMode) {
                            case EncryptionMode.AES:
                                AesKey tokenKey = HandleAesKeyExtractorUI(model.TokenKeyIdentifier, "Token");
                                if (tokenKey == null)
                                    return;
                                tokenService.ManipulateStream(o => o.UseBase64()
                                .UseCryptography(EncryptionMode.AES)
                                .ProvideKey(tokenKey)
                                .Set());
                                break;
                            case EncryptionMode.WindowsDataProtectionAPI:
                                tokenService.ManipulateStream(o => o.UseBase64()
                                .UseCryptography(EncryptionMode.WindowsDataProtectionAPI)
                                .Set());
                                break;
                        }

                        tokens = ReadTokens().ToArray();
                    }
                    else {
                        TokenEntryModel tokenEntry = new TokenEntryModel();
                        TokenEntryView view = new TokenEntryView { DataContext = new TokenEntryViewModel(tokenEntry) };
                        UIHelper.Show(view);
                        if (tokenEntry.IsCanceled)
                            return;

                        tokens = tokenEntry.Tokens;
                    }

                    entityServiceHandler = DIContainer.GetService<ICachedDiscordEntityServiceHandler>();
                    entityServiceHandler.Init(tokens);

                    if (model.EncryptData) {
                        switch (model.DataEncryptionMode) {
                            case EncryptionMode.AES:
                                AesKey dataKey = HandleAesKeyExtractorUI(model.DataKeyIdentifier, "Data");
                                if (dataKey == null)
                                    return;

                                entityServiceHandler.ManipulateStream(o => o.UseBase64()
                                .UseCryptography(EncryptionMode.AES)
                                .ProvideKey(dataKey)
                                .Set());
                                break;
                            case EncryptionMode.WindowsDataProtectionAPI:
                                entityServiceHandler.ManipulateStream(o => o.UseBase64()
                                .UseCryptography(EncryptionMode.WindowsDataProtectionAPI)
                                .Set());
                                break;
                        }
                    }

                    await entityServiceHandler.Refresh();
                    entityServiceHandler.Dispose();
                    UIHelper.ShowInfo("Server Image generated.", "Success");
                }
                catch (InternalException ex) {
                    this.logger.LogError(ex.ToString());
                    UIHelper.ShowError("Image generation failed.", "Error");
                    return;
                }
            });
        }

        private IEnumerable<TokenModel> ReadTokens() {
            string[] files = Directory.GetFiles(DiscordEnvironment.TokenCache);
            foreach(string file in files) {
                yield return tokenService.ReadToken(file);
            }
        }

        private AesKey HandleAesKeyExtractorUI(Guid? id, string name) {
            KeyEntryModel keyEntry = new KeyEntryModel(name);
            KeyEntryView view = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            logger.LogInformation($"Requesting aes key for {name}.");
            UIHelper.Show(view);
            if (keyEntry.Key == null) {
                logger.LogInformation("Request cancelled.");
                return null;
            }

            if (!keyEntry.Key.Identify(id.GetValueOrDefault())) {
                string error = $"Wrong key for {name} provided.";
                UIHelper.ShowError(error, "Wrong key");
                this.logger.LogError(error);
                return null;
            }

            return keyEntry.Key.Reference;
        }
    }
}
