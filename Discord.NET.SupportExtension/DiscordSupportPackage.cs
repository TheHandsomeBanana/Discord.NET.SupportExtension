using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.Models.Collections;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService.Holder;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Unity;
using HB.NETF.VisualStudio.UI;
using HB.NETF.VisualStudio.Workspace;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Unity;
using Task = System.Threading.Tasks.Task;

namespace Discord.NET.SupportExtension {
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.DiscordSupportPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)] // Auto load if solution exists
    public sealed class DiscordSupportPackage : AsyncPackage {

        public static string EventLogPath = DiscordEnvironment.LogPath + "\\" + DateTime.Now.ToString("yyyy.MM.dd_HHmmss") + ".log";
        private readonly ILogger<DiscordSupportPackage> logger;
        public DiscordSupportPackage() {
            UIHelper.Package = this;

            // Setup DI Container
            UnityBase.Boot(
                new HB.NETF.Discord.NET.Toolkit.UnitySetup(),
                new HB.NETF.Services.Data.UnitySetup(),
                new HB.NETF.Services.Logging.UnitySetup(),
                new HB.NETF.Services.Security.UnitySetup(),
                new HB.NETF.Services.Serialization.UnitySetup());

            IUnityContainer container = UnityBase.CreateChildContainer(nameof(DiscordSupportPackage)); // Add child container for Unity
            UnityBase.Boot(container,
                new UnitySetup(),
                new Core.UnitySetup());


            ILoggerFactory loggerFactory = UnityBase.UnityContainer.Resolve<ILoggerFactory>();
            loggerFactory.InvokeLoggingBuilder(b => b
               .AddTarget(EventLogPath)
               .AddTarget(UIHelper.OutputWindowFunc));

            this.logger = loggerFactory.GetOrCreateLogger<DiscordSupportPackage>();
        }



        public static string GetCachePath() => DiscordEnvironment.CachePath + "\\" + SolutionHelper.GetCurrentProject().Name + DiscordEnvironment.CacheExtension;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {

            IMenuCommandService commandService = await this.GetServiceAsync(typeof(IMenuCommandService)) as IMenuCommandService;

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            UIHelper.InitOutputLog("Discord Support Extension");

            Task initConfig = GenerateServerImageConfigurationCommand.InitializeAsync(this, commandService, OnException);
            Task initGenerate = GenerateServerImageCommand.InitializeAsync(this, commandService, OnException);
            Task initLoad = LoadServerCollectionCommand.InitializeAsync(this, commandService, OnException);
            await Task.WhenAll(initConfig, initGenerate, initLoad);


            // Safe Package Load
            try {
                string projectName = SolutionHelper.GetCurrentProject().Name;
                string configPath = ConfigHelper.GetConfigPath();

                if (File.Exists(configPath))
                    await HandleCacheAsync(projectName, configPath);
                else
                    logger.LogInformation(InteractionMessages.ConfigurationNotFoundFor(projectName) + ", " + InteractionMessages.ServerCollectionNotLoaded);
            }
            catch (Exception ex) {
                logger.LogError(ex.Message);
            }
        }

        private void OnException(Exception exception) {
            logger.LogError(exception.ToString());
            UIHelper.ShowError(exception.Message);
        }

        private async Task HandleCacheAsync(string projectName, string configPath) {
            ILogger<DiscordSupportPackage> logger = UnityBase.UnityContainer.Resolve<ILoggerFactory>().GetOrCreateLogger<DiscordSupportPackage>();
            IAsyncStreamHandler streamHandler = UnityBase.UnityContainer.Resolve<IAsyncStreamHandler>();
            IDiscordEntityService entityService = UnityBase.UnityContainer.Resolve<IDiscordEntityService>(nameof(DiscordRestEntityService));
            ConfigureServerImageModel model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(configPath);

            InteractionHelper.MapDataEncryptToEntityService(entityService, model, logger, out bool cancel);
            if (cancel) {
                logger.LogWarning(InteractionMessages.CouldNotRetrieveAesKey + ", " + InteractionMessages.ServerCollectionNotLoadedFor(projectName));
                return;
            }

            try {
                DiscordServerCollection serverCollection = await entityService.ReadFromFile(GetCachePath());
                UnityBase.UnityContainer.Resolve<IServerCollectionHolder>()
                    .Hold(projectName, serverCollection);

                logger.LogInformation(InteractionMessages.ServerCollectionLoadedFor(projectName));
            }
            catch {
                logger.LogError(InteractionMessages.ServerCollectionNotLoadedFor(projectName) + ", " + InteractionMessages.GenerateNewServerImage);
                throw;
            }
        }

        #endregion
    }
}
