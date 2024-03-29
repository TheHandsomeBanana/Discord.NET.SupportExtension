﻿using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Common.Exceptions;
using HB.NETF.Services.Data.Handler;
using HB.NETF.Services.Data.Identifier;
using HB.NETF.Services.Logging;
using HB.NETF.Services.Logging.Factory;
using HB.NETF.Services.Security.Cryptography.Keys;
using HB.NETF.Unity;
using HB.NETF.WPF.Commands;
using HB.NETF.WPF.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using Unity;

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
        [Dependency]
        public IStreamHandler StreamHandler { get; set; }
        private readonly ILogger<DiscordSupportPackage> logger;
        public KeyEntryViewModel(KeyEntryModel model) {
            ExitCommand = new RelayCommand(Exit, null);
            BrowseKeyCommand = new RelayCommand(BrowseKey, null);
            ExtractCommand = new RelayCommand(Extract, (o) => !string.IsNullOrWhiteSpace(KeyPath) && File.Exists(KeyPath));
            this.model = model;
            this.logger = UnityBase.GetChildContainer(nameof(DiscordSupportPackage)).Resolve<ILoggerFactory>().GetOrCreateLogger<DiscordSupportPackage>();
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
                model.Key = StreamHandler.ReadFromFile<Identifier<AesKey>>(KeyPath);
                Close?.Invoke();
            }
            catch (InternalException ex) {
                logger.LogError(ex.ToString());
            }
        }
    }
}
