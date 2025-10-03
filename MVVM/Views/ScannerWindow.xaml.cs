using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    public partial class ScannerWindow : Window
    {
        public ScannerWindow()
        {
            InitializeComponent();
            DataContext = new ScannerViewModel(); // INGEN PARAMETRE
        }
    }
}