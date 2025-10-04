using System;
using System.Windows;
using ReolMarked.MVVM.Database;

namespace ReolMarked
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var dbConfig = DatabaseConfiguration.Instance;
                if (dbConfig.TestConnection())
                {
                    MessageBox.Show("Database forbindelse OK!", "Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Kunne ikke forbinde til databasen", "Fejl",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database fejl: {ex.Message}", "Fejl",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}