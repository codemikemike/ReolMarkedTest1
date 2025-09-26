using ReolMarked.MVVM.ViewModels;
using ReolMarkedTest1.MVVM.Views;
using System.Windows;

namespace ReolMarked.MVVM.Views
{
    /// <summary>
    /// Code-behind for MainWindow
    /// Holdes minimal i MVVM - kun DataContext opsætning
    /// </summary>
    public partial class MainWindow : Window
    {
        // Konstruktør - opsætter kun DataContext
        public MainWindow()
        {
            // Initialiser UI komponenter fra XAML
            InitializeComponent();

            // Sæt ViewModel som DataContext for databinding
            // Dette gør at {Binding} i XAML virker
            DataContext = new MainViewModel();
        }
        /// <summary>
        /// Åbner stregkode genererings vinduet (UC3.2)
        /// </summary>
        private void OpenBarcodeWindow_Click(object sender, RoutedEventArgs e)
        {
            var barcodeWindow = new BarcodeWindow();
            barcodeWindow.Show();
        }

        /// <summary>
        /// Åbner scanner kasse vinduet (UC3.1)
        /// </summary>
        private void OpenScannerWindow_Click(object sender, RoutedEventArgs e)
        {
            var scannerWindow = new ScannerWindow();
            scannerWindow.Show();
        }
        private void OpenFakturaWindow_Click(object sender, RoutedEventArgs e)
        {
            var fakturaWindow = new FakturaWindow();
            fakturaWindow.Show();
        }
    }
}