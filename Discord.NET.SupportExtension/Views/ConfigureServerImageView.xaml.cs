using HB.NETF.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Discord.NET.SupportExtension.Views {
    /// <summary>
    /// Interaction logic for ConfigureServerImageView.xaml
    /// </summary>
    public partial class ConfigureServerImageView : Window {
        public ConfigureServerImageView() {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext is ICloseableWindow vm) {
                vm.Close += () => {
                    this.Close();
                };

                Closing += (s, c) => {
                    c.Cancel = !vm.CanClose();
                };
            }
        }
    }
}
