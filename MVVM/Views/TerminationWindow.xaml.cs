using System.Windows;
using ReolMarked.MVVM.ViewModels;

namespace ReolMarked.MVVM.Views
{
    public partial class TerminationWindow : Window
    {
        public TerminationWindow()
        {
            InitializeComponent();
            DataContext = new TerminationViewModel(); // INGEN PARAMETRE
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}