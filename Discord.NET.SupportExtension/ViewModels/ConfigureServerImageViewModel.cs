using Discord.NET.SupportExtension.Commands;
using Discord.NET.SupportExtension.Models.ConfigurationModel;
using EnvDTE;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Serialization;
using HB.NETF.Discord.NET.Toolkit.DataService;
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
            }
        }

        private Visibility tokenAESEncryptionPanelVisibility;

        public Visibility TokenAESEncryptionPanelVisibility {
            get { return tokenAESEncryptionPanelVisibility; }
            set {
                tokenAESEncryptionPanelVisibility = value;
                OnPropertyChanged(nameof(TokenAESEncryptionPanelVisibility));
            }
        }

        private int dataEncryptionMethod;

        public int DataEncryptionMethod {
            get { return (int)model.DataEncryptionMethod; }
            set {
                model.DataEncryptionMethod = (EncryptionMethod)value;
                OnPropertyChanged(nameof(DataEncryptionMethod));

                if (value == (int)EncryptionMethod.AES && EncryptData) {
                    DataAESEncryptionPanelVisibility = Visibility.Visible;
                }
                else
                    DataAESEncryptionPanelVisibility = Visibility.Collapsed;
            }
        }


        public int TokenEncryptionMethod {
            get { return (int)model.TokenEncryptionMethod; }
            set {
                model.TokenEncryptionMethod = (EncryptionMethod)value;
                OnPropertyChanged(nameof(TokenEncryptionMethod));

                if (value == (int)EncryptionMethod.AES && SaveTokens)
                    TokenAESEncryptionPanelVisibility = Visibility.Visible;
                else
                    TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion


        private readonly ISimplifiedSerializerService serializerService;
        private readonly IDiscordDataServiceWrapper discordDataService;
        private readonly Project currentProject;
        private readonly ConfigureServerImageModel model;
        public Action Close { get; set; }

        public ConfigureServerImageViewModel(ConfigureServerImageModel model) {
            this.serializerService = DIContainer.GetService<ISimplifiedSerializerService>();
            this.currentProject = SolutionHelper.GetCurrentProject();
            this.discordDataService = DIContainer.GetService<IDiscordDataServiceWrapper>();

            GenerateCommand = new AsyncRelayCommand(GenerateServerImageAsync, null, (e) => { });
            SaveCommand = new RelayCommand(Save, null);
            ExitCommand = new RelayCommand(Exit, null);
            DataAESEncryptionPanelVisibility = Visibility.Collapsed;
            TokenAESEncryptionPanelVisibility = Visibility.Collapsed;
            this.model = model;
        }

        private async Task GenerateServerImageAsync() {
            
            await discordDataService.DownloadDataAsync();

        }

        private void Save(object o) {
            ThreadHelper.ThrowIfNotOnUIThread();

            string targetLocation = Path.GetDirectoryName(this.currentProject.FullName) + "\\" + "DiscordServerImageConfig.json";
            serializerService.Write(targetLocation, model, SerializerMode.Json);

            ProjectHelper.AddExistingFile(currentProject, targetLocation);
            Exit(o);
        }

        private void Exit(object o) {
            Close?.Invoke();
        }

        public bool CanClose() => true;
    }
}
