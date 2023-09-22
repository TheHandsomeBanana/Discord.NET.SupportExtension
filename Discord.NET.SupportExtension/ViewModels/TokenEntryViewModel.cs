using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.WPF.Base.CommandBase;
using HB.NETF.WPF.Base.ViewModelBase;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Discord.NET.SupportExtension.ViewModels {
    public class TokenEntryViewModel : ViewModelBase, ICloseableWindow {
        #region Binding
        public ICommand ExitCommand { get; }
        public ICommand FinishCommand { get; }
        #endregion

        public Action Close { get; set; }

        private TokenEntryModel model;
        public TokenEntryViewModel(TokenEntryModel model) {
            this.model = model;

            this.ExitCommand = new RelayCommand(Exit, null);
            this.FinishCommand = new RelayCommand(Finish, null);
        }

        private void Finish(object obj) {

            this.Exit(obj);
        }

        private void Exit(object obj) {
            Close?.Invoke();
        }

        public bool CanClose() => true;
    }
}
