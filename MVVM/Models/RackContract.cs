using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for en reol kontrakt
    /// Binder en kunde til en reol med lejebetingelser
    /// </summary>
    public class RackContract : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _contractId;
        private int _customerId;
        private int _rackId;
        private DateTime _startDate;
        private DateTime _endDate;
        private decimal _monthlyRent;
        private bool _isActive;
        private DateTime _createdAt;
        private string _notes;

        // Navigation properties - disse vil blive fyldt fra Repository
        private Customer _customer;
        private Rack _rack;

        // Konstruktør - opretter en ny kontrakt
        public RackContract()
        {
            // Sæt standard værdier
            StartDate = DateTime.Now.Date; // I dag
            EndDate = DateTime.Now.Date.AddMonths(12); // 1 år frem
            MonthlyRent = 850; // Standard pris for 1 reol
            IsActive = true; // Nye kontrakter er aktive
            CreatedAt = DateTime.Now;
            Notes = "";
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for kontrakten i databasen
        /// </summary>
        public int ContractId
        {
            get { return _contractId; }
            set
            {
                _contractId = value;
                OnPropertyChanged(nameof(ContractId));
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
        /// Hvornår lejemålet slutter
        /// </summary>
        public DateTime EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                OnPropertyChanged(nameof(EndDateFormatted));
            }
        }

        /// <summary>
        /// Månedlig leje i kroner
        /// </summary>
        public decimal MonthlyRent
        {
            get { return _monthlyRent; }
            set
            {
                _monthlyRent = value;
                OnPropertyChanged(nameof(MonthlyRent));
                OnPropertyChanged(nameof(MonthlyRentFormatted));
            }
        }

        /// <summary>
        /// Om kontrakten stadig er aktiv
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        /// <summary>
        /// Hvornår kontrakten blev oprettet
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
        /// Noter til kontrakten
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
        /// Kunden der har kontrakten
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
        /// Formateret slut dato
        /// </summary>
        public string EndDateFormatted
        {
            get { return EndDate.ToString("dd/MM/yyyy"); }
        }

        /// <summary>
        /// Formateret månedlig leje
        /// </summary>
        public string MonthlyRentFormatted
        {
            get { return $"{MonthlyRent:C0}"; } // Dansk valuta format
        }

        /// <summary>
        /// Status tekst for kontrakten
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsActive) return "Inaktiv";
                if (DateTime.Now.Date > EndDate.Date) return "Udløbet";
                if (DateTime.Now.Date < StartDate.Date) return "Ikke startet";
                return "Aktiv";
            }
        }

        /// <summary>
        /// Kontraktens display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                string customerName = Customer?.CustomerName ?? "Ukendt kunde";
                string rackNumber = Rack?.RackNumber.ToString() ?? "Ukendt reol";
                return $"Kontrakt {ContractId} - {customerName} - Reol {rackNumber}";
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
                MonthlyRent = 850;
            else if (totalRacksForCustomer >= 2 && totalRacksForCustomer <= 3)
                MonthlyRent = 825;
            else if (totalRacksForCustomer >= 4)
                MonthlyRent = 800;
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