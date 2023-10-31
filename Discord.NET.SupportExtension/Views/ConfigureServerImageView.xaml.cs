using HB.NETF.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            if(DataContext is ICloseableWindow vm) {
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
