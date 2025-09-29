using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    /// <summary>
    /// Code-behind for TerminationWindow
    /// Minimal MVVM - kun window håndtering
    /// </summary>
    public partial class TerminationWindow : Window
    {
        public TerminationWindow()
        {
            InitializeComponent();

            // Sæt ViewModel som DataContext
            DataContext = new TerminationViewModel();
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