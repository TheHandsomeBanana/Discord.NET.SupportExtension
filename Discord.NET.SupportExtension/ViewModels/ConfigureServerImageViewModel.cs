using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Helper;
using Discord.NET.SupportExtension.Models.ConfigurationModel;
using EnvDTE;
using HB.NETF.Common;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Serialization;
using HB.NETF.Discord.NET.Toolkit;
using HB.NETF.Discord.NET.Toolkit.DataService;
using HB.NETF.Discord.NET.Toolkit.DataService.Models;
using HB.NETF.Services.Security.Cryptography.Interfaces;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Services.Security.DataHandling;
using HB.NETF.Services.Security.DataProtection;
using HB.NETF.Services.Storage;
using HB.NETF.VisualStudio.Workspace;
using HB.NETF.WPF.Base.CommandBase;
using HB.NETF.WPF.Base.ViewModelBase;
using HB.NETF.WPF.Exceptions;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
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


        private Visibility dataAESEncryptionPanelVisibility;

        public Visibility DataAESEncryptionPanelVisibility {
            get { return dataAESEncryptionPanelVisibility; }
            set {
                dataAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(DataAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.CanExecute(null);
            }
        }

        private Visibility tokenAESEncryptionPanelVisibility;

        public Visibility TokenAESEncryptionPanelVisibility {
            get { return tokenAESEncryptionPanelVisibility; }
            set {
                tokenAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(TokenAESEncryptionPanelVisibility));
                CreateDataAESKeyFileCommand.CanExecute(null);
            }
        }

        private AesKey dataKey;
        public int DataEncryptionMethod {
            get { return (int)model.DataEncryptionMethod; }
            set {
                model.DataEncryptionMethod = (EncryptionMethod)value;
                OnPropertyChanged(nameof(DataEncryptionMethod));

                if (value == (int)EncryptionMethod.AES && EncryptData) {
                    DataAESEncryptionPanelVisibility = Visibility.Visible;
                    dataKey = cryptoService.GenerateKeys(256)[0];
                    DataAESKey = Convert.ToBase64String(dataKey.Key);
                }
                else {
                    DataAESEncryptionPanelVisibility = Visibility.Collapsed;
                    DataAESKey = null;
                    dataKey = null;
                }
            }
        }

        private AesKey tokenKey;
        public int TokenEncryptionMethod {
            get { return (int)model.TokenEncryptionMethod; }
            set {
                model.TokenEncryptionMethod = (EncryptionMethod)value;
                OnPropertyChanged(nameof(TokenEncryptionMethod));

                if (value == (int)EncryptionMethod.AES && SaveTokens) {
                    TokenAESEncryptionPanelVisibility = Visibility.Visible;
                    tokenKey = cryptoService.GenerateKeys(256)[0];
                    TokenAESKey = Convert.ToBase64String(tokenKey.Key);
                }
                else {
                    TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
                    TokenAESKey = null;
                    tokenKey = null;
                }
            }
        }

        #endregion

        #endregion


        private readonly ISimplifiedSerializerService serializerService;
        private readonly IDiscordDataServiceWrapper discordDataService;
        private readonly ICryptoService<AesKey> cryptoService;
        private readonly IDataProtectionService dataProtectionService;
        private readonly Project currentProject;
        private readonly ConfigureServerImageModel model;
        public Action Close { get; set; }

        public ConfigureServerImageViewModel(ConfigureServerImageModel model) {
            this.serializerService = DIContainer.GetService<ISimplifiedSerializerService>();
            this.currentProject = SolutionHelper.GetCurrentProject();
            this.discordDataService = DIContainer.GetService<IDiscordDataServiceWrapper>();
            this.cryptoService = DIContainer.GetService<ICryptoService<AesKey>>();
            this.dataProtectionService = DIContainer.GetService<IDataProtectionService>();

            GenerateCommand = new AsyncRelayCommand(GenerateServerImageAsync, null, (e) => { });
            SaveCommand = new RelayCommand(Save, null);
            ExitCommand = new RelayCommand(Exit, null);
            CreateTokenAESKeyFileCommand = new RelayCommand(CreateTokenAESKeyFile, null);
            CreateDataAESKeyFileCommand = new RelayCommand(CreateDataAESKeyFile, null);
            DataAESEncryptionPanelVisibility = Visibility.Collapsed;
            TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
            this.model = model;
        }

        #region Command Callbacks

        private async Task GenerateServerImageAsync() {


            await discordDataService.DownloadDataAsync();

        }

        private void Save(object o) {
            ThreadHelper.ThrowIfNotOnUIThread();

            string configLocation = ConfigHelper.GetConfigPath(this.currentProject);
            serializerService.Write(configLocation, model, SerializerMode.Json);

            ProjectHelper.AddExistingFile(currentProject, configLocation);
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

        private void CreateAESKeyFile(AesKey key) {
            using (KeyStreamHandler keyStream = new KeyStreamHandler(typeof(AesKey))) {
                keyStream.Write(key);
            }
        }

        #endregion

        public bool CanClose() => true;
    }
}
