using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Result klasse for stregkode generering operationer
    /// Returneres fra BarcodeService når labels oprettes
    /// </summary>
    public class BarcodeGenerationResult : INotifyPropertyChanged
    {
        // Private felter
        private bool _success;
        private string _errorMessage = "";
        private ObservableCollection<Label> _createdLabels = new();
        private string _printOutput = "";

        /// <summary>
        /// Om operationen lykkedes
        /// </summary>
        public bool Success
        {
            get { return _success; }
            set
            {
                _success = value;
                OnPropertyChanged(nameof(Success));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Fejlbesked hvis operationen fejlede
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(HasError));
            }
        }

        /// <summary>
        /// Liste over oprettede labels
        /// </summary>
        public ObservableCollection<Label> CreatedLabels
        {
            get { return _createdLabels; }
            set
            {
                _createdLabels = value;
                OnPropertyChanged(nameof(CreatedLabels));
                OnPropertyChanged(nameof(LabelCount));
                OnPropertyChanged(nameof(HasLabels));
            }
        }

        /// <summary>
        /// Print output tekst til stregkoder
        /// </summary>
        public string PrintOutput
        {
            get { return _printOutput; }
            set
            {
                _printOutput = value;
                OnPropertyChanged(nameof(PrintOutput));
                OnPropertyChanged(nameof(HasPrintOutput));
            }
        }

        // Beregnet properties

        /// <summary>
        /// Om der er en fejl
        /// </summary>
        public bool HasError
        {
            get { return !Success && !string.IsNullOrEmpty(ErrorMessage); }
        }

        /// <summary>
        /// Antal oprettede labels
        /// </summary>
        public int LabelCount
        {
            get { return CreatedLabels?.Count ?? 0; }
        }

        /// <summary>
        /// Om der er oprettet labels
        /// </summary>
        public bool HasLabels
        {
            get { return LabelCount > 0; }
        }

        /// <summary>
        /// Om der er print output
        /// </summary>
        public bool HasPrintOutput
        {
            get { return !string.IsNullOrEmpty(PrintOutput); }
        }

        /// <summary>
        /// Status tekst til UI
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!Success && HasError)
                    return $"Fejl: {ErrorMessage}";

                if (Success && HasLabels)
                    return $"Succesfuldt oprettet {LabelCount} stregkode(r)";

                if (Success)
                    return "Operation gennemført";

                return "Ukendt status";
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}