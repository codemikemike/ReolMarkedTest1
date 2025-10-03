using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(); // INGEN PARAMETRE
        }
    }
}