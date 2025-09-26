using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for en reol lejeaftale
    /// Binder en kunde til en reol med lejebetingelser
    /// </summary>
    public class RentalAgreement : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _agreementId;
        private int _customerId;
        private int _rackId;
        private DateTime _startDate;
        private decimal _price;
        private string _status;
        private DateTime _createdAt;
        private string _notes;

        // Navigation properties - disse vil blive fyldt fra Service
        private Customer _customer;
        private Rack _rack;

        // Konstruktør - opretter en ny lejeaftale
        public RentalAgreement()
        {
            // Sæt standard værdier
            StartDate = DateTime.Now.Date; // I dag
            Price = 850; // Standard pris for 1 reol
            Status = "Active"; // Nye aftaler er aktive
            CreatedAt = DateTime.Now;
            Notes = "";
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for lejeaftalen i databasen
        /// </summary>
        public int AgreementId
        {
            get { return _agreementId; }
            set
            {
                _agreementId = value;
                OnPropertyChanged(nameof(AgreementId));
            }
        }

        /// <summary>
        /// ID på kunden der lejer reolen
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
        /// ID på reolen der lejes
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
        /// Hvornår lejemålet starter
        /// </summary>
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(StartDateFormatted));
            }
        }

        /// <summary>
        /// Månedlig leje i kroner
        /// </summary>
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(PriceFormatted));
            }
        }

        /// <summary>
        /// Status for lejeaftalen (Active, Inactive, Expired)
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsActive));
            }
        }

        /// <summary>
        /// Hvornår aftalen blev oprettet
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
        /// Noter til aftalen
        /// </summary>
        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        // Navigation properties
        /// <summary>
        /// Kunden der har aftalen
        /// </summary>
        public Customer Customer
        {
            get { return _customer; }
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
            }
        }

        /// <summary>
        /// Reolen der lejes
        /// </summary>
        public Rack Rack
        {
            get { return _rack; }
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(Rack));
            }
        }

        // Beregnet properties for UI visning

        /// <summary>
        /// Formateret start dato
        /// </summary>
        public string StartDateFormatted
        {
            get { return StartDate.ToString("dd/MM/yyyy"); }
        }

        /// <summary>
        /// Formateret månedlig leje
        /// </summary>
        public string PriceFormatted
        {
            get { return $"{Price:C0}"; } // Dansk valuta format
        }

        /// <summary>
        /// Om aftalen er aktiv
        /// </summary>
        public bool IsActive
        {
            get { return Status == "Active"; }
        }

        /// <summary>
        /// Aftalens display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                string customerName = Customer?.CustomerName ?? "Ukendt kunde";
                string rackNumber = Rack?.RackNumber.ToString() ?? "Ukendt reol";
                return $"Aftale {AgreementId} - {customerName} - Reol {rackNumber}";
            }
        }

        // Metode til at beregne rabat baseret på antal reoler
        /// <summary>
        /// Beregner månedlig leje baseret på antal reoler kunden lejer
        /// 1 reol: 850 kr, 2-3 reoler: 825 kr, 4+ reoler: 800 kr
        /// </summary>
        public void CalculateRentDiscount(int totalRacksForCustomer)
        {
            if (totalRacksForCustomer == 1)
                Price = 850;
            else if (totalRacksForCustomer >= 2 && totalRacksForCustomer <= 3)
                Price = 825;
            else if (totalRacksForCustomer >= 4)
                Price = 800;
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