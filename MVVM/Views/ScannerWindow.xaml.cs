using System.Windows;

namespace ReolMarkedTest1.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ScannerWindow.xaml
    /// </summary>
    public partial class ScannerWindow : Window
    {
        public ScannerWindow()
        {
            InitializeComponent();
            // DataContext sættes automatisk i XAML via Window.DataContext
        }
    }
}