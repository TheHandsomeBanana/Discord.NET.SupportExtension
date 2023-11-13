using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models;
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
using HB.NETF.VisualStudio.Commands;
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
    internal sealed class GenerateServerImageCommand : AsyncCommandBase {
        protected override Guid CommandSet => PackageGuids.CommandSet;
        protected override int CommandId => PackageIds.GenerateServerImageCommand;

        private static IAsyncStreamHandler streamHandler;
        private readonly ILogger<GenerateServerImageCommand> logger;

        internal GenerateServerImageCommand(AsyncPackage package, OleMenuCommandService commandService, Action<Exception> onException) : base(package, commandService, onException) {
            streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<GenerateServerImageCommand>();
        }

        public static GenerateServerImageCommand Instance { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package, Action<Exception> onException) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new GenerateServerImageCommand(package, commandService, onException);
        }

        protected override async Task ExecuteAsync(object sender, EventArgs e) {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            string currentConfigPath = ConfigHelper.GetConfigPath();
            string currentCachePath = DiscordSupportPackage.GetCachePath();

            await Package.JoinableTaskFactory.RunAsync(async () => {
                ConfigureServerImageModel model = new ConfigureServerImageModel();

                DateTime startedAt = DateTime.Now;
                JobStatus status = JobStatus.Failed;

                IDiscordTokenService tokenService = DIContainer.GetService<IDiscordTokenService>();

                string token;

                try {
                    if (File.Exists(ConfigHelper.GetConfigPath()))
                        model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(currentConfigPath);

                    token = InteractionHelper.GetDecryptedToken(tokenService, model, logger);

                    if (string.IsNullOrWhiteSpace(token)) {
                        logger.LogInformation("Get token failed, server image generation aborted.");
                        return;
                    }

                    IDiscordEntityService entityService = DIContainer.GetService<IDiscordEntityService>();
                    entityService.OnTimeout += OnTimeout;

                    InteractionHelper.MapDataEncryptToEntityService(entityService, model, logger, out bool cancel);
                    if (cancel) {
                        logger.LogInformation("Server image generation cancelled.");
                        return;
                    }

                    await entityService.Connect(token);
                    if (entityService.Ready) {
                        await entityService.LoadEntities();
                        await entityService.SaveToFile(currentCachePath);
                        logger.LogInformation("Server image successfully generated.");
                        UIHelper.ShowInfo("Server Image generated.", "Success");

                        status = JobStatus.Succeeded;
                    }

                    entityService.Dispose();
                }
                catch (InternalException ex) {
                    this.logger.LogError(ex.ToString());
                    UIHelper.ShowError("Server image generation failed.", "Error");
                }
                finally {
                    DateTime finishedAt = DateTime.Now;
                    model.RunLog.Add(new RunLogEntry() { StartedAt = startedAt, FinishedAt = finishedAt, Status = status });
                    await streamHandler.WriteToFileAsync(ConfigHelper.GetConfigPath(), model);
                }
            });
        }

        private void OnTimeout() {
            logger.LogError("Could not generate image, connection timed out");
            UIHelper.ShowError("Could not generate image.", "Failure");
        }
    }
}
