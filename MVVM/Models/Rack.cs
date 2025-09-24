using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for en reol i reolmarkedet
    /// Indeholder grundlæggende information om reolen
    /// </summary>
    public class Rack : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _rackId;
        private int _rackNumber;
        private bool _hasHangerBar;
        private int _amountShelves;
        private string _location;
        private bool _isAvailable;
        private string _description;

        // Konstruktør - opretter en ny reol
        public Rack()
        {
            // Sæt standard værdier
            AmountShelves = 6; // Som standard har reoler 6 hylder
            IsAvailable = true; // Nye reoler er ledige
            HasHangerBar = false; // Som standard ingen bøjlestang
            Location = "Ikke angivet";
            Description = "";
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for reolen i databasen
        /// </summary>
        public int RackId
        {
            get { return _rackId; }
            set
            {
                _rackId = value;
                OnPropertyChanged(nameof(RackId));
            }
        }

        /// <summary>
        /// Reolnummer som kunder kan se (f.eks. 1-80)
        /// </summary>
        public int RackNumber
        {
            get { return _rackNumber; }
            set
            {
                _rackNumber = value;
                OnPropertyChanged(nameof(RackNumber));
            }
        }

        /// <summary>
        /// Om reolen har bøjlestang til tøj
        /// </summary>
        public bool HasHangerBar
        {
            get { return _hasHangerBar; }
            set
            {
                _hasHangerBar = value;
                OnPropertyChanged(nameof(HasHangerBar));
            }
        }

        /// <summary>
        /// Antal hylder i reolen
        /// </summary>
        public int AmountShelves
        {
            get { return _amountShelves; }
            set
            {
                _amountShelves = value;
                OnPropertyChanged(nameof(AmountShelves));
            }
        }

        /// <summary>
        /// Reolens placering i butikken
        /// </summary>
        public string Location
        {
            get { return _location; }
            set
            {
                _location = value;
                OnPropertyChanged(nameof(Location));
            }
        }

        /// <summary>
        /// Om reolen er ledig til leje
        /// </summary>
        public bool IsAvailable
        {
            get { return _isAvailable; }
            set
            {
                _isAvailable = value;
                OnPropertyChanged(nameof(IsAvailable));
            }
        }

        /// <summary>
        /// Beskrivelse af reolen (ekstra info)
        /// </summary>
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        // Beregnet property for at vise reol type
        /// <summary>
        /// Viser hvilken type reol det er
        /// </summary>
        public string RackType
        {
            get
            {
                if (HasHangerBar)
                    return $"{AmountShelves} hylder + bøjlestang";
                else
                    return $"{AmountShelves} hylder";
            }
        }

        // Beregnet property for status tekst
        /// <summary>
        /// Viser status for reolen
        /// </summary>
        public string StatusText
        {
            get
            {
                return IsAvailable ? "Ledig" : "Optaget";
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sender besked når en property ændres (til databinding)
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}