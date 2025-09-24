using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for en kunde i reolmarkedet
    /// Indeholder kundens grundlæggende oplysninger
    /// </summary>
    public class Customer : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _customerId;
        private string _customerName = "";
        private string _customerPhone = "";
        private string _customerEmail = "";
        private string _customerAddress = "";
        private DateTime _createdAt;
        private bool _isActive;

        // Konstruktør - opretter en ny kunde
        public Customer()
        {
            // Sæt standard værdier
            CreatedAt = DateTime.Now; // Nuværende tidspunkt
            IsActive = true; // Nye kunder er aktive
            CustomerName = "";
            CustomerPhone = "";
            CustomerEmail = "";
            CustomerAddress = "";
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for kunden i databasen
        /// </summary>
        public int CustomerId
        {
            get { return _customerId; }
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        /// <summary>
        /// Kundens fulde navn (f.eks. "Anton Mikkelsen")
        /// </summary>
        public string CustomerName
        {
            get { return _customerName; }
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        /// <summary>
        /// Kundens telefonnummer
        /// </summary>
        public string CustomerPhone
        {
            get { return _customerPhone; }
            set
            {
                _customerPhone = value;
                OnPropertyChanged(nameof(CustomerPhone));
            }
        }

        /// <summary>
        /// Kundens email adresse
        /// </summary>
        public string CustomerEmail
        {
            get { return _customerEmail; }
            set
            {
                _customerEmail = value;
                OnPropertyChanged(nameof(CustomerEmail));
            }
        }

        /// <summary>
        /// Kundens adresse
        /// </summary>
        public string CustomerAddress
        {
            get { return _customerAddress; }
            set
            {
                _customerAddress = value;
                OnPropertyChanged(nameof(CustomerAddress));
            }
        }

        /// <summary>
        /// Hvornår kunden blev oprettet
        /// </summary>
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }

        /// <summary>
        /// Om kunden stadig er aktiv
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        // Beregnet property for visning i UI
        /// <summary>
        /// Formateret dato for hvornår kunden blev oprettet
        /// </summary>
        public string CreatedAtFormatted
        {
            get
            {
                return CreatedAt.ToString("dd/MM/yyyy");
            }
        }

        /// <summary>
        /// Status tekst for kunden
        /// </summary>
        public string StatusText
        {
            get
            {
                return IsActive ? "Aktiv" : "Inaktiv";
            }
        }

        /// <summary>
        /// Kort visning af kundens info til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"{CustomerName} - {CustomerPhone}";
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sender besked når en property ændres (til databinding)
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}