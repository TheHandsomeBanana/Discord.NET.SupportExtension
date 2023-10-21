using Discord.NET.SupportExtension.Models.VMModels;
using HB.NETF.Discord.NET.Toolkit.Obsolete.EntityService.Models;
using HB.NETF.WPF.Base.CommandBase;
using HB.NETF.WPF.Base.ViewModelBase;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Discord.NET.SupportExtension.ViewModels {
    public class TokenEntryViewModel : ViewModelBase, ICloseableWindow {
        #region Binding
        public RelayCommand AddTokenCommand { get; }
        public RelayCommand RemoveTokenCommand { get; }
        public RelayCommand EditTokenCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand FinishCommand { get; }

        public IEnumerable<string> Tokens => model.Tokens;

        private int selectedTokenIndex = -1;
        public int SelectedTokenIndex {
            get => selectedTokenIndex;
            set {
                selectedTokenIndex = value;
                OnPropertyChanged(nameof(SelectedTokenIndex));
                EditTokenCommand.OnCanExecuteChanged();
                RemoveTokenCommand.OnCanExecuteChanged();
            }
        }

        private string tokenText;
        public string TokenText {
            get { return tokenText; }
            set {
                tokenText = value;
                OnPropertyChanged(nameof(TokenText));
                AddTokenCommand.OnCanExecuteChanged();
            }
        }
        #endregion

        public Action Close { get; set; }

        private readonly TokenEntryModel model;
        public TokenEntryViewModel(TokenEntryModel model) {
            this.model = model;
            AddTokenCommand = new RelayCommand(AddToken, o => !string.IsNullOrWhiteSpace(TokenText) && TokenText.Length == 70);
            RemoveTokenCommand = new RelayCommand(RemoveToken, (o) => SelectedTokenIndex > -1);
            EditTokenCommand = new RelayCommand(EditToken, (o) => SelectedTokenIndex > -1);
            this.ExitCommand = new RelayCommand(Exit, null);
            this.FinishCommand = new RelayCommand(Finish, o => model.Tokens.Count > 0);
        }

        private void Finish(object obj) {
            Close?.Invoke();
        }

        private void Exit(object obj) {
            model.IsCanceled = true;
            Close?.Invoke();
        }

        private void EditToken(object obj) {
            TokenText = model.Tokens[selectedTokenIndex];
            RemoveToken(obj);
        }

        private void RemoveToken(object obj) {
            model.Tokens.RemoveAt(selectedTokenIndex);
            FinishCommand.OnCanExecuteChanged();
        }

        private void AddToken(object obj) {
            model.Tokens.Add(TokenText);
            TokenText = "";
            FinishCommand.OnCanExecuteChanged();
        }

        public bool CanClose() => true;
    }
}
