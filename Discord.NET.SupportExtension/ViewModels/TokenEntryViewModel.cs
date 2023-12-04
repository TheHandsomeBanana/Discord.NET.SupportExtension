using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.WPF.Commands;
using HB.NETF.WPF.ViewModels;
using System;

namespace Discord.NET.SupportExtension.ViewModels {
    public class TokenEntryViewModel : ViewModelBase, ICloseableWindow {
        #region Binding
        public RelayCommand ExitCommand { get; }
        public RelayCommand FinishCommand { get; }


        public string TokenText {
            get { return model.Token; }
            set {
                model.Token = value;
                OnPropertyChanged(nameof(TokenText));
            }
        }
        #endregion

        public Action Close { get; set; }

        private readonly TokenEntryModel model;
        public TokenEntryViewModel(TokenEntryModel model) {
            this.model = model;
            this.ExitCommand = new RelayCommand(Exit, null);
            this.FinishCommand = new RelayCommand(Finish, o => model.Token != null);
        }

        private void Finish(object obj) {
            Close?.Invoke();
        }

        private void Exit(object obj) {
            model.IsCanceled = true;
            Finish(obj);
        }

        public bool CanClose() => true;
    }
}
