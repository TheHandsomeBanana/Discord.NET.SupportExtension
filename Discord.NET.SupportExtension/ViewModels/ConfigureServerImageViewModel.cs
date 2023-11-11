using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.Obsolete.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.Obsolete.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.Obsolete.TokenService;
using HB.NETF.Services.Data.Exceptions;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Handler.Async;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Services.Security.Cryptography.Settings;
using HB.NETF.Services.Security.DataProtection;
using HB.NETF.VisualStudio.Commands;
using HB.NETF.VisualStudio.UI;
using HB.NETF.VisualStudio.Workspace;
using HB.NETF.WPF.Base.CommandBase;
using HB.NETF.WPF.ViewModels;
using HB.NETF.WPF.Exceptions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Microsoft.VisualStudio.Shell.RegistrationAttribute;
using HB.NETF.WPF.Commands;

namespace Discord.NET.SupportExtension.ViewModels {
    public class ConfigureServerImageViewModel : ViewModelBase, ICloseableWindow {

        #region Bindings
        #region Commands
        public RelayCommand GenerateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand CreateDataAESKeyFileCommand { get; }
        public RelayCommand CreateTokenAESKeyFileCommand { get; }
        public RelayCommand SaveTokenCommand { get; }
        #endregion

        #region UI Only
        private string dataAESKey;
        public string DataAESKey {
            get {
                return dataAESKey;
            }
            set {
                dataAESKey = value;
                OnPropertyChanged(nameof(DataAESKey));
            }
        }
        private string tokenAESKey;
        public string TokenAESKey {
            get {
                return tokenAESKey;
            }
            set {
                tokenAESKey = value;
                OnPropertyChanged(nameof(TokenAESKey));
            }
        }
        #endregion

        #region Settings
        public bool EncryptData {
            get { return model.EncryptData; }
            set {
                model.EncryptData = value;
                OnPropertyChanged(nameof(EncryptData));

                if (value)
                    DataEncryptionMode = 0;
                else
                    DataEncryptionMode = null;
            }
        }

        public bool SaveToken {
            get { return model.SaveToken; }
            set {
                model.SaveToken = value;
                OnPropertyChanged(nameof(SaveToken));

                if (value)
                    TokenEncryptionMode = 0;
                else
                    TokenEncryptionMode = null;
            }
        }


        private Visibility dataAESEncryptionPanelVisibility = Visibility.Collapsed;

        public Visibility DataAESEncryptionPanelVisibility {
            get { return dataAESEncryptionPanelVisibility; }
            set {
                dataAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(DataAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.OnCanExecuteChanged();
            }
        }

        private Visibility tokenAESEncryptionPanelVisibility = Visibility.Collapsed;

        public Visibility TokenAESEncryptionPanelVisibility {
            get { return tokenAESEncryptionPanelVisibility; }
            set {
                tokenAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(TokenAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.OnCanExecuteChanged();
            }
        }

        public Array EncryptionModeValues => Enum.GetValues(typeof(EncryptionMode));

        private Identifier<AesKey> tokenKey;
        public EncryptionMode? TokenEncryptionMode {
            get {
                return model.TokenEncryptionMode;
            }
            set {
                model.TokenEncryptionMode = value;
                OnPropertyChanged(nameof(TokenEncryptionMode));
                SetTokenModeDependent();
            }
        }

        private Identifier<AesKey> dataKey;
        public EncryptionMode? DataEncryptionMode {
            get {
                return model.DataEncryptionMode;
            }
            set {
                model.DataEncryptionMode = value;
                OnPropertyChanged(nameof(DataEncryptionMode));
                SetDataModeDependent();
            }
        }
        #endregion

        #region Token
        private TokenModel token;
        public TokenModel Token {
            get {
                return token;
            }
            set {
                token = value;
            }
        }
        #endregion
        #endregion

        private readonly ILogger<DiscordSupportPackage> logger;
        private readonly IAesCryptoService aesCryptoService;
        private readonly IAsyncStreamHandler streamHandler;
        private readonly IDiscordTokenService tokenService;
        private readonly IIdentifierFactory identifierFactory;
        private readonly Project currentProject;
        private readonly ConfigureServerImageModel model;

        public Action Close { get; set; }

        public ConfigureServerImageViewModel(ConfigureServerImageModel model) {
            this.logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<DiscordSupportPackage>();
            this.streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            this.currentProject = SolutionHelper.GetCurrentProject();
            this.identifierFactory = DIContainer.GetService<IIdentifierFactory>();
            this.tokenService = DIContainer.GetService<IDiscordTokenService>();
            this.aesCryptoService = DIContainer.GetService<IAesCryptoService>();

            GenerateCommand = new RelayCommand(GenerateServerImage, null);
            SaveCommand = new RelayCommand(Save, null);
            ExitCommand = new RelayCommand(Exit, null);
            CreateTokenAESKeyFileCommand = new RelayCommand(CreateTokenAESKeyFile, null);
            CreateDataAESKeyFileCommand = new RelayCommand(CreateDataAESKeyFile, null);
            SaveTokenCommand = new RelayCommand(SaveTokensToModel, null);
            this.model = model;
        }

        #region Command Callbacks
        private void GenerateServerImage(object o) {
            CommandHelper.RunVSCommand(PackageGuids.CommandSet, PackageIds.GenerateServerImageCommand);
        }

        private void Save(object o) {
            ThreadHelper.ThrowIfNotOnUIThread();
            if(!SaveToken && model.Token != null) {
                logger.LogWarning($"{nameof(SaveToken)} is disabled, there is still a token saved. Saving will remove it and you will need a new key if aes encryption is used.");
                int option = UIHelper.ShowWarningWithCancel("If you save now, your current token will be removed.", "Save Tokens is disabled");
                if (option == 1) {
                    model.Token = null;
                    logger.LogInformation("Token removed.");
                }
                else
                    return;
            }

            string configLocation = ConfigHelper.GetConfigPath(this.currentProject);
            streamHandler.WriteToFile(configLocation, model);

            ProjectHelper.AddExistingFile(this.currentProject, configLocation);
            logger.LogInformation($"Configuration saved to project {this.currentProject.Name}");
            Exit(o);
        }

        private void Exit(object o) {
            Close?.Invoke();
        }

        private void CreateDataAESKeyFile(object o) {
            CreateAESKeyFile(dataKey);
        }

        private void CreateTokenAESKeyFile(object o) {
            CreateAESKeyFile(tokenKey);
        }

        private void SaveTokensToModel(object obj) {
            switch (model.TokenEncryptionMode) {
                case EncryptionMode.AES:
                    AesKey key = GetAesKey();
                    if (key == null)
                        return;

                    model.Token = tokenService.EncryptToken(this.token, model.TokenEncryptionMode.Value, key);
                    break;
                case EncryptionMode.WindowsDataProtectionAPI:
                    model.Token = tokenService.EncryptToken(this.token, model.TokenEncryptionMode.Value);
                    break;
            }

            logger.LogInformation($"Token encrypted and saved.");
            UIHelper.ShowInfo("Token saved successfully", "Info");
        }
        #endregion

        public bool CanClose() => true;
        private void CreateAESKeyFile(Identifier<AesKey> key) {
            try {
                streamHandler.StartSaveFileDialog(key);
            }
            catch (StreamHandlerException e) {
                string error = "Could not create key file";
                logger.LogError(error + e.Message);
                UIHelper.ShowError(error, "Error");
            }
        }

        private AesKey GetAesKey() {
            KeyEntryModel keyEntry = new KeyEntryModel("Token");
            KeyEntryView keyEntryView = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            this.logger.LogInformation("Requesting key for tokens.");
            UIHelper.Show(keyEntryView);
            if (keyEntry.IsCanceled) {
                this.logger.LogInformation($"Request for tokens cancelled.");
                return null;
            }

            if (!keyEntry.ContainsKey()) {
                this.logger.LogInformation($"No key found.");
                return null;
            }

            if (!keyEntry.Key.Identify(model.TokenKeyIdentifier.GetValueOrDefault())) {
                string error = $"Wrong key for token provided.";
                UIHelper.ShowError(error, "Wrong key");
                this.logger.LogError(error);
                return null;
            }

            return keyEntry.Key.Reference;
        }

        private void SetTokenModeDependent() {
            if (model.TokenEncryptionMode == EncryptionMode.AES && SaveToken) {
                TokenAESEncryptionPanelVisibility = Visibility.Visible;
                tokenKey = identifierFactory.CreateIdentifier(aesCryptoService.GenerateKey(256));
                TokenAESKey = Convert.ToBase64String(tokenKey.Reference.Key);
                model.TokenKeyIdentifier = tokenKey.Id;
            }
            else {
                TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
                TokenAESKey = null;
                tokenKey = null;
                model.TokenKeyIdentifier = null;
            }
        }
        private void SetDataModeDependent() {
            if (model.DataEncryptionMode == EncryptionMode.AES && EncryptData) {
                DataAESEncryptionPanelVisibility = Visibility.Visible;
                dataKey = identifierFactory.CreateIdentifier(aesCryptoService.GenerateKey(256));
                DataAESKey = Convert.ToBase64String(dataKey.Reference.Key);
                model.DataKeyIdentifier = dataKey.Id;
            }
            else {
                DataAESEncryptionPanelVisibility = Visibility.Collapsed;
                DataAESKey = null;
                dataKey = null;
                model.DataKeyIdentifier = null;
            }
        }
    }
}
