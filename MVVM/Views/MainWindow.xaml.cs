using System.Windows;
using ReolMarked.MVVM.ViewModels;

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
    }
}