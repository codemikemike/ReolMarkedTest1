using ReolMarked.MVVM.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ReolMarkedTest1.MVVM.Views
{
    /// <summary>
    /// Code-behind for ScannerWindow
    /// Minimal MVVM - kun window og keyboard håndtering
    /// </summary>
    public partial class ScannerWindow : Window
    {
        public ScannerWindow()
        {
            InitializeComponent();

            // Sæt ViewModel som DataContext
            DataContext = new ScannerViewModel();
        }

        /// <summary>
        /// Håndterer Enter key i stregkode input (simulerer scanner)
        /// </summary>
        private void BarcodeInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Trigger scan kommando når Enter trykkes
                var viewModel = DataContext as ScannerViewModel;
                if (viewModel?.ScanBarcodeCommand.CanExecute(null) == true)
                {
                    viewModel.ScanBarcodeCommand.Execute(null);
                }
            }
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