using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    /// <summary>
    /// Code-behind for FakturaWindow
    /// Minimal MVVM - kun window håndtering
    /// </summary>
    public partial class FakturaWindow : Window
    {
        public FakturaWindow()
        {
            InitializeComponent();

            // Sæt ViewModel som DataContext
            DataContext = new FakturaViewModel();
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