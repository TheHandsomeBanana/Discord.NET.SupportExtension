using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit;
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
using HB.NETF.Discord.NET.Toolkit.Services.TokenService;
using Discord.NET.SupportExtension.Models;

namespace Discord.NET.SupportExtension.ViewModels {
    public class ConfigureServerImageViewModel : ViewModelBase, ICloseableWindow {

        #region Bindings
        #region Commands
        public RelayCommand GenerateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand CreateDataAESKeyFileCommand { get; }
        public RelayCommand CreateTokenAESKeyFileCommand { get; }
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
                if (!value && CheckExistingToken()) {
                    if (!DeleteTokenDialog())
                        return;

                    model.Token = null;
                    this.Token = null;
                    model.RunLog.Clear();
                }



                model.SaveToken = value;
                OnPropertyChanged(nameof(SaveToken));

                if (value)
                    TokenEncryptionMode = 0;
                else
                    TokenEncryptionMode = null;
            }
        }

        private bool CheckExistingToken() {
            return model.Token != null || token != null;
        }

        private bool DeleteTokenDialog() {
            int res = UIHelper.ShowWarningWithCancel(InteractionMessages.TokenAndRunLogWillBeRemoved, "Existing token warning");
            return res == 1;
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

        #region Data
        private string token;
        public string Token {
            get {
                return token;
            }
            set {
                token = value;
                OnPropertyChanged(nameof(Token));
            }
        }

        public IEnumerable<RunLogEntry> RunLog => model.RunLog;
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
            this.model = model;

            if (model.Token != null)
                LoadTokenFromModel();
        }

        #region Command Callbacks
        private void GenerateServerImage(object o) {
            Save(o);
            CommandHelper.RunVSCommand(PackageGuids.CommandSet, PackageIds.GenerateServerImageCommand);
        }

        private void Save(object o) {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!ValidateToken())
                return;

            if (SaveToken)
                SaveTokenToModel();

            string configLocation = ConfigHelper.GetConfigPath(this.currentProject);
            streamHandler.WriteToFile(configLocation, model);

            ProjectHelper.AddExistingFile(this.currentProject, configLocation);
            logger.LogInformation(InteractionMessages.ConfigurationSavedTo(this.currentProject.Name));
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

        private void SaveTokenToModel() {
            switch (model.TokenEncryptionMode) {
                case EncryptionMode.AES:
                    AesKey key = InteractionHelper.GetAesKeyFromUIInput(model.TokenKeyIdentifier.GetValueOrDefault(), "Token", this.logger);
                    if (key == null)
                        return;

                    model.Token = tokenService.EncryptToken(this.Token, model.TokenEncryptionMode.Value, key);
                    break;
                case EncryptionMode.WindowsDataProtectionAPI:
                    model.Token = tokenService.EncryptToken(this.Token, model.TokenEncryptionMode.Value);
                    break;
            }

            logger.LogInformation(InteractionMessages.TokenSaved);
        }
        private void LoadTokenFromModel() {
            switch (model.TokenEncryptionMode) {
                case EncryptionMode.AES:
                    AesKey key = InteractionHelper.GetAesKeyFromUIInput(model.TokenKeyIdentifier.GetValueOrDefault(), "Token", this.logger);
                    if (key == null)
                        return;

                    this.Token = tokenService.DecryptToken(model.Token, model.TokenEncryptionMode.Value, key);
                    break;
                case EncryptionMode.WindowsDataProtectionAPI:
                    this.Token = tokenService.DecryptToken(model.Token, model.TokenEncryptionMode.Value);
                    break;
            }
        }
        private bool ValidateToken() {
            if (!SaveToken)
                return true;

            if (token == null) {
                UIHelper.ShowError(InteractionMessages.NoTokenToSave + ", " + InteractionMessages.ProvideToken, "Token missing");
                return false;
            }

            if (string.IsNullOrWhiteSpace(token) || token.Length != 70) {
                UIHelper.ShowError(InteractionMessages.TokenInvalid, "Token invalid");
                return false;
            }

            return true;
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
