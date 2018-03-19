using Cryptography.ViewModel;
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

namespace Cryptography.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void buttonDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Operacja zakończyła się niepowodzeniem", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

            

            
        }

        private void buttonBrowseSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog();
            var result = openFileDialog.ShowDialog();
        }

        private void buttonBrowseDestinationFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderdialog = new System.Windows.Forms.FolderBrowserDialog();
            var folder = folderdialog.ShowDialog();
        }
    }
}
