using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    public partial class BarcodeWindow : Window
    {
        public BarcodeWindow()
        {
            InitializeComponent();
            DataContext = new BarcodeViewModel(); // INGEN PARAMETRE
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}