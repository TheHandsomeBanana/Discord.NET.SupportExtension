using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Common;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.Services.EntityService;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Services.Security.Cryptography.Settings;
using HB.NETF.VisualStudio.UI;
using HB.NETF.VisualStudio.Workspace;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
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
        private ILogger<DiscordSupportPackage> logger;
        public DiscordSupportPackage() {
            UIHelper.Package = this;

            // Setup DI Container
            DIBuilder builder = new DIBuilder();
            new DIConfig().Configure(builder);
            new Core.DIConfig().Configure(builder);
            DIContainer.BuildServiceProvider(builder);

            ILoggerFactory loggerFactory = DIContainer.GetService<ILoggerFactory>();
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
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            UIHelper.InitOutputLog("Discord Support Extension");
            await GenerateServerImageConfigurationCommand.InitializeAsync(this);
            await GenerateServerImageCommand.InitializeAsync(this);


            // Safe Package Load
            try {
                if (File.Exists(ConfigHelper.GetConfigPath()))
                    await HandleCacheAsync();
                else
                    logger.LogInformation($"No config file found in active project {SolutionHelper.GetCurrentProject().Name}. ServerCollection not loaded.");
            }
            catch(Exception ex) {
                logger.LogError(ex.Message);
            }
        }

        private async Task HandleCacheAsync() {
            ILogger<DiscordSupportPackage> logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<DiscordSupportPackage>();
            IAsyncStreamHandler streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            IDiscordEntityService mergedEntityService = DIContainer.GetService<IDiscordEntityService>();
            ConfigureServerImageModel model = await streamHandler.ReadFromFileAsync<ConfigureServerImageModel>(ConfigHelper.GetConfigPath());

            GenerateHelper.ManipulateEntityServiceDataEncrypt(mergedEntityService, model, logger, out bool cancel);
            if (cancel) {
                logger.LogWarning("Could not retrieve aes key for data decryption. ServerCollection not loaded.");
                return;
            }

            if (await mergedEntityService.ReadFromFile(GetCachePath()))
                logger.LogInformation("ServerCollection loaded.");
            else
                logger.LogWarning("ServerCollection not loaded. Generate new server image.");
        }

        #endregion
    }
}
