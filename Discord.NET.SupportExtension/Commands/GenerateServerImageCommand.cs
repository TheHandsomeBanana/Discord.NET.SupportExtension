using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models;
using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Common.Exceptions;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Unity;
using HB.NETF.VisualStudio.Commands;
using HB.NETF.VisualStudio.UI;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using Unity;
using Task = System.Threading.Tasks.Task;

namespace Discord.NET.SupportExtension.Commands {
    internal sealed class GenerateServerImageCommand : AsyncCommandBase {
        protected override Guid CommandSet => PackageGuids.CommandSet;
        protected override int CommandId => PackageIds.GenerateServerImageCommand;


        [Dependency]
        public IAsyncStreamHandler StreamHandler { get; set; }
        [Dependency]
        public ILoggerFactory LoggerFactory { get; set; }

        private readonly IUnityContainer container;
        private readonly ILogger<GenerateServerImageCommand> logger;
        internal GenerateServerImageCommand(AsyncPackage package, IMenuCommandService commandService, Action<Exception> onException) : base(package, commandService, onException) {
            container = UnityBase.GetChildContainer(nameof(DiscordSupportPackage));
            container.BuildUp(this);

            logger = LoggerFactory.GetOrCreateLogger<GenerateServerImageCommand>();
        }

        public static GenerateServerImageCommand Instance { get; private set; }
        public static async Task InitializeAsync(AsyncPackage package, IMenuCommandService commandService, Action<Exception> onException) {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            Instance = new GenerateServerImageCommand(package, commandService, onException);


        }

        protected override async Task ExecuteAsync(object sender, EventArgs e) {
            await Package.JoinableTaskFactory.SwitchToMainThreadAsync();
            string currentConfigPath = ConfigHelper.GetConfigPath();
            string currentCachePath = DiscordSupportPackage.GetCachePath();

            await Package.JoinableTaskFactory.RunAsync((Func<Task>)(async () => {
                ConfigureServerImageModel model = new ConfigureServerImageModel();

                DateTime startedAt = DateTime.Now;
                JobStatus status = JobStatus.Failed;

                IDiscordTokenService tokenService = container.Resolve<IDiscordTokenService>();

                string token;

                try {
                    if (File.Exists(ConfigHelper.GetConfigPath()))
                        model = await this.StreamHandler.ReadFromFileAsync<ConfigureServerImageModel>(currentConfigPath);

                    token = InteractionHelper.GetDecryptedToken(tokenService, model, logger);

                    if (string.IsNullOrWhiteSpace(token)) {
                        logger.LogInformation(InteractionMessages.GetTokenFailed + ", " + InteractionMessages.GenerationAborted);
                        return;
                    }

                    IDiscordEntityService entityService = container.Resolve<IDiscordEntityService>(nameof(DiscordRestEntityService));
                    try {
                        entityService.OnTimeout += OnTimeout;

                        InteractionHelper.MapDataEncryptToEntityService(entityService, model, logger, out bool cancel);
                        if (cancel) {
                            logger.LogInformation(InteractionMessages.GenerationAborted);
                            return;
                        }

                        await entityService.Connect(token);
                        if (entityService.Ready) {
                            DiscordServerCollection serverCollection = await entityService.LoadEntities();

                            await entityService.SaveToFile(currentCachePath, serverCollection);
                            logger.LogInformation(InteractionMessages.GenerateImageSuccess);
                            UIHelper.ShowInfo(InteractionMessages.GenerateImageSuccess);

                            CommandHelper.RunVSCommand(CommandSet, PackageIds.LoadServerCollectionCommand);

                            status = JobStatus.Succeeded;
                        }
                        await entityService.Disconnect();
                    }
                    finally {
                        await entityService.DisposeAsync();
                    }
                }
                catch (InternalException ex) {
                    this.logger.LogError(ex.ToString());
                    UIHelper.ShowError(InteractionMessages.GenerateImageFailure);
                }
                finally {
                    DateTime finishedAt = DateTime.Now;
                    model.RunLog.Add(new RunLogEntry() { StartedAt = startedAt, FinishedAt = finishedAt, Status = status });
                    await this.StreamHandler.WriteToFileAsync<ConfigureServerImageModel>(currentConfigPath, model);
                }
            }));
        }

        private void OnTimeout() {
            logger.LogError(InteractionMessages.GenerateImageFailure + ", " + InteractionMessages.ConnectionTimeout);
            UIHelper.ShowError(InteractionMessages.GenerateImageFailure);
        }
    }
}
