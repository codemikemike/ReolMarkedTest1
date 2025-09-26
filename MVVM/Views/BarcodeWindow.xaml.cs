using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    /// <summary>
    /// Code-behind for BarcodeWindow
    /// Minimal MVVM - kun window håndtering
    /// </summary>
    public partial class BarcodeWindow : Window
    {
        public BarcodeWindow()
        {
            InitializeComponent();

            // Sæt ViewModel som DataContext
            DataContext = new BarcodeViewModel();
        }

        /// <summary>
        /// Luk vindue når Luk knap trykkes
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}