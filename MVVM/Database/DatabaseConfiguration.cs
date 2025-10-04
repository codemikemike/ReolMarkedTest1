using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace ReolMarked.MVVM.Database
{
    /// <summary>
    /// Håndterer indlæsning af konfiguration fra appsettings.json
    /// </summary>
    public class DatabaseConfiguration
    {
        private static DatabaseConfiguration? _instance;
        private readonly IConfiguration _configuration;
        private static readonly object _lock = new object();

        private DatabaseConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        /// <summary>
        /// Singleton instance af DatabaseConfiguration
        /// </summary>
        public static DatabaseConfiguration Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseConfiguration();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Henter connection string fra appsettings.json
        /// </summary>
        public string ConnectionString =>
            _configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");

        /// <summary>
        /// Henter app settings
        /// </summary>
        public T GetAppSetting<T>(string key)
        {
            var value = _configuration[$"AppSettings:{key}"];
            if (value == null)
                throw new InvalidOperationException($"App setting '{key}' not found");

            return (T)Convert.ChangeType(value, typeof(T));
        }

        /// <summary>
        /// Test database forbindelse
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
                connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection failed: {ex.Message}");
                return false;
            }
        }
    }
}