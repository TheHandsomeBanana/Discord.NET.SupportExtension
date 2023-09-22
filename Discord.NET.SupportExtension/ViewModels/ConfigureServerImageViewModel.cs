using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.VMModels;
using Discord.NET.SupportExtension.Views;
using EnvDTE;
using HB.NETF.Common;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.EntityService.Handler;
using HB.NETF.Discord.NET.Toolkit.EntityService.Models;
using HB.NETF.Discord.NET.Toolkit.TokenService;
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
using HB.NETF.WPF.Base.ViewModelBase;
using HB.NETF.WPF.Exceptions;
using Microsoft.VisualStudio.Shell;
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

namespace Discord.NET.SupportExtension.ViewModels {
    public class ConfigureServerImageViewModel : ViewModelBase, ICloseableWindow {

        #region Bindings
        public ICommand GenerateCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand CreateDataAESKeyFileCommand { get; }
        public ICommand CreateTokenAESKeyFileCommand { get; }
        public ICommand LoadTokensCommand { get; }
        public ICommand AddTokenCommand { get; }
        public ICommand RemoveTokenCommand { get; }
        public ICommand EditTokenCommand { get; }

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
                    DataEncryptionMethod = 0;
                else
                    DataEncryptionMethod = -1;
            }
        }

        public bool SaveTokens {
            get { return model.SaveTokens; }
            set {
                model.SaveTokens = value;
                OnPropertyChanged(nameof(SaveTokens));

                if (value)
                    TokenEncryptionMethod = 0;
                else
                    TokenEncryptionMethod = -1;
            }
        }


        private Visibility dataAESEncryptionPanelVisibility = Visibility.Collapsed;

        public Visibility DataAESEncryptionPanelVisibility {
            get { return dataAESEncryptionPanelVisibility; }
            set {
                dataAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(DataAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.CanExecute(null);
            }
        }

        private Visibility tokenAESEncryptionPanelVisibility = Visibility.Collapsed;

        public Visibility TokenAESEncryptionPanelVisibility {
            get { return tokenAESEncryptionPanelVisibility; }
            set {
                tokenAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(TokenAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.CanExecute(null);
            }
        }

        private Identifier<AesKey> dataKey;
        public int DataEncryptionMethod {
            get {
                if (model.DataEncryptionMode == null)
                    return -1;

                return (int)model.DataEncryptionMode;
            }
            set {
                model.DataEncryptionMode = (EncryptionMode)value;
                OnPropertyChanged(nameof(DataEncryptionMethod));

                if (model.DataEncryptionMode == EncryptionMode.AES && EncryptData) {
                    DataAESEncryptionPanelVisibility = Visibility.Visible;
                    dataKey = identifierFactory.CreateIdentifier(aesCryptoService.GenerateKey(256));
                    DataAESKey = Convert.ToBase64String(dataKey.Reference.Key);
                }
                else {
                    DataAESEncryptionPanelVisibility = Visibility.Collapsed;
                    DataAESKey = null;
                    dataKey = null;
                }
            }
        }

        public Array EncryptionModeValues = Enum.GetValues(typeof(EncryptionMode));
        public EncryptionMode? TokenEncryptionMode {
            get {
                return model.TokenEncryptionMode;
            }
            set {
                model.TokenEncryptionMode = value;
                OnPropertyChanged(nameof(TokenEncryptionMode));
            }
        }

        public EncryptionMode? DataEncryptionMode {
            get {
                return model.DataEncryptionMode;
            }
            set {
                model.DataEncryptionMode = value;
                OnPropertyChanged(nameof(DataEncryptionMode));
            }
        }


        private Identifier<AesKey> tokenKey;
        public int TokenEncryptionMethod {
            get {
                if (model.TokenEncryptionMode == null)
                    return -1;

                return (int)model.TokenEncryptionMode;
            }
            set {
                model.TokenEncryptionMode = (EncryptionMode)value;
                OnPropertyChanged(nameof(TokenEncryptionMethod));

                if (model.TokenEncryptionMode == EncryptionMode.AES && SaveTokens) {
                    TokenAESEncryptionPanelVisibility = Visibility.Visible;
                    tokenKey = identifierFactory.CreateIdentifier(aesCryptoService.GenerateKey(256));
                    TokenAESKey = Convert.ToBase64String(tokenKey.Reference.Key);
                }
                else {
                    TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
                    TokenAESKey = null;
                    tokenKey = null;
                }
            }
        }

        #endregion

        #region Token
        private ObservableCollection<TokenModel> tokens = new ObservableCollection<TokenModel>();
        public IEnumerable<TokenModel> Tokens => tokens;

        private int selectedTokenIndex;
        public int SelectedTokenIndex {
            get => selectedTokenIndex;
            set {
                selectedTokenIndex = value;
                OnPropertyChanged(nameof(SelectedTokenIndex));
                EditTokenCommand.CanExecute(null);
                RemoveTokenCommand.CanExecute(null);
            }
        }
        #endregion
        #endregion

        private readonly ILogger<ConfigureServerImageViewModel> logger;
        private readonly IAesCryptoService aesCryptoService;
        private readonly IAsyncStreamHandler streamHandler;
        private readonly IDiscordEntityServiceHandler entityService;
        private readonly IDiscordTokenService tokenService;
        private readonly IIdentifierFactory identifierFactory;
        private readonly Project currentProject;
        private readonly ConfigureServerImageModel model;
        public Action Close { get; set; }

        public ConfigureServerImageViewModel(ConfigureServerImageModel model) {
            this.logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<ConfigureServerImageViewModel>();
            this.streamHandler = DIContainer.GetService<IAsyncStreamHandler>();
            this.currentProject = SolutionHelper.GetCurrentProject();
            this.entityService = DIContainer.GetService<IDiscordEntityServiceHandler>();
            this.identifierFactory = DIContainer.GetService<IIdentifierFactory>();
            this.tokenService = DIContainer.GetService<IDiscordTokenService>();
            this.aesCryptoService = DIContainer.GetService<IAesCryptoService>();

            GenerateCommand = new RelayCommand(GenerateServerImage, null);
            SaveCommand = new RelayCommand(Save, null);
            ExitCommand = new RelayCommand(Exit, null);
            CreateTokenAESKeyFileCommand = new RelayCommand(CreateTokenAESKeyFile, null);
            CreateDataAESKeyFileCommand = new RelayCommand(CreateDataAESKeyFile, null);
            LoadTokensCommand = new RelayCommand(LoadTokens, null);
            AddTokenCommand = new RelayCommand(AddToken, null);
            RemoveTokenCommand = new RelayCommand(RemoveToken, (o) => SelectedTokenIndex > -1);
            EditTokenCommand = new RelayCommand(EditToken, (o) => SelectedTokenIndex > -1);
            this.model = model;
        }

        private void EditToken(object obj) {
            throw new NotImplementedException();
        }

        private void RemoveToken(object obj) {
            throw new NotImplementedException();
        }

        private void AddToken(object obj) {
            throw new NotImplementedException();
        }

        private void LoadTokens(object obj) {
            switch(TokenEncryptionMethod) {

            }
            KeyEntryModel keyEntry = new KeyEntryModel("Token");
            KeyEntryView keyEntryView = new KeyEntryView() { DataContext = new KeyEntryViewModel(keyEntry) };
            this.logger.LogInformation("Requesting key for tokens.");
            UIHelper.Show(keyEntryView);
            if (keyEntry.IsCanceled) {
                this.logger.LogInformation($"Request for tokens cancelled.");
                return;
            }

            if (!keyEntry.ContainsKey()) {
                this.logger.LogInformation($"No key found.");
                return;
            }

            if (!keyEntry.Key.Identify(model.TokenKeyIdentifier.GetValueOrDefault())) {
                string error = $"Wrong key for token provided.";
                UIHelper.ShowError(error, "Wrong key");
                this.logger.LogError(error);
                return;
            }

            this.tokens = ReadTokens(keyEntry.Key.Reference);
        }

        #region Command Callbacks

        private void GenerateServerImage(object o) {
            CommandHelper.RunVSCommand(PackageGuids.CommandSet, PackageIds.GenerateServerImageCommand);
        }

        private void Save(object o) {
            ThreadHelper.ThrowIfNotOnUIThread();

            string configLocation = ConfigHelper.GetConfigPath(this.currentProject);
            streamHandler.WriteToFile(configLocation, model);

            ProjectHelper.AddExistingFile(this.currentProject, configLocation);
            logger.LogInformation($"New configuration saved to project {this.currentProject.Name}");
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

        private ObservableCollection<TokenModel> ReadTokens(AesKey key) {
            //tokenService.ManipulateStream((o) => o.UseBase64().UseCryptography(EncryptionMode)

            List<TokenModel> tokens = new List<TokenModel>();
            IEnumerable<string> files = Directory.GetFiles(DiscordEnvironment.TokenCache);
            foreach (string file in files) {
                tokens.Add(tokenService.ReadToken(file));
            }

            return new ObservableCollection<TokenModel>();
        }
    }
}
