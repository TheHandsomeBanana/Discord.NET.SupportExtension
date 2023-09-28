using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Common.DependencyInjection;
using HB.NETF.Common.Exceptions;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.WPF.Base.CommandBase;
using HB.NETF.WPF.Base.ViewModelBase;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Discord.NET.SupportExtension.ViewModels {
    internal class KeyEntryViewModel : ViewModelBase, ICloseableWindow {


        #region Bindings
        public RelayCommand ExtractCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand BrowseKeyCommand { get; }

        #region UI Only
        private string keyPath;
        public string KeyPath {
            get { return keyPath; }
            set {
                keyPath = value;
                OnPropertyChanged(nameof(KeyPath));
                ExtractCommand.OnCanExecuteChanged();
            }
        }
        #endregion

        public string Name {
            get { return model.Name + " Key"; }
            set {
                model.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        #endregion

        public Action Close { get; set; }

        private readonly KeyEntryModel model;
        private readonly IStreamHandler streamHandler;
        private readonly ILogger<DiscordSupportPackage> logger;
        public KeyEntryViewModel(KeyEntryModel model) {
            ExitCommand = new RelayCommand(Exit, null);
            BrowseKeyCommand = new RelayCommand(BrowseKey, null);
            ExtractCommand = new RelayCommand(Extract, (o) => !string.IsNullOrWhiteSpace(KeyPath) && File.Exists(KeyPath));
            this.model = model;
            this.streamHandler = DIContainer.GetService<IStreamHandler>();
            this.logger = DIContainer.GetService<ILoggerFactory>().GetOrCreateLogger<DiscordSupportPackage>();
        }

        public bool CanClose() => true;

        private void Exit(object o) {
            model.IsCanceled = true;
            Close?.Invoke();
        }

        private void BrowseKey(object o) {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!ofd.ShowDialog().Value)
                return;

            KeyPath = ofd.FileName;
        }

        private void Extract(object o) {
            try {
                model.Key = streamHandler.ReadFromFile<Identifier<AesKey>>(KeyPath);
                Close?.Invoke();
            }
            catch (InternalException ex) {
                logger.LogError(ex.ToString());
            }
        }
    }
}
