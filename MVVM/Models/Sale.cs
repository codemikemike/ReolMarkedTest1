using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for et salg i reolmarkedet
    /// Repræsenterer Ayas køb af flere produkter
    /// </summary>
    public class Sale : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _saleId;
        private DateTime _datoTid;
        private decimal _total;
        private string _betalingsForm = "";
        private decimal _betaltBelob;
        private decimal _byttePenge;
        private bool _isCompleted;
        private string _notes = "";

        // Navigation properties
        private ObservableCollection<SaleLine> _saleLines = new();

        // Konstruktør - opretter et nyt salg
        public Sale()
        {
            // Sæt standard værdier
            DatoTid = DateTime.Now;
            IsCompleted = false;
            BetalingsForm = "Kontant"; // Standard betalingsform
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for salget
        /// </summary>
        public int SaleId
        {
            get { return _saleId; }
            set
            {
                _saleId = value;
                OnPropertyChanged(nameof(SaleId));
            }
        }

        /// <summary>
        /// Dato og tid for salget
        /// </summary>
        public DateTime DatoTid
        {
            get { return _datoTid; }
            set
            {
                _datoTid = value;
                OnPropertyChanged(nameof(DatoTid));
                OnPropertyChanged(nameof(DatoTidFormatted));
            }
        }

        /// <summary>
        /// Samlet beløb for salget
        /// </summary>
        public decimal Total
        {
            get { return _total; }
            set
            {
                _total = value;
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(TotalFormatted));
                CalculateByttePenge();
            }
        }

        /// <summary>
        /// Betalingsform (Kontant, MobilePay, osv.)
        /// </summary>
        public string BetalingsForm
        {
            get { return _betalingsForm; }
            set
            {
                _betalingsForm = value;
                OnPropertyChanged(nameof(BetalingsForm));
            }
        }

        /// <summary>
        /// Beløb kunden har betalt
        /// </summary>
        public decimal BetaltBelob
        {
            get { return _betaltBelob; }
            set
            {
                _betaltBelob = value;
                OnPropertyChanged(nameof(BetaltBelob));
                OnPropertyChanged(nameof(BetaltBelobFormatted));
                CalculateByttePenge();
            }
        }

        /// <summary>
        /// Byttepenge til kunden
        /// </summary>
        public decimal ByttePenge
        {
            get { return _byttePenge; }
            set
            {
                _byttePenge = value;
                OnPropertyChanged(nameof(ByttePenge));
                OnPropertyChanged(nameof(ByttePengeFormatted));
            }
        }

        /// <summary>
        /// Om salget er gennemført
        /// </summary>
        public bool IsCompleted
        {
            get { return _isCompleted; }
            set
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        /// <summary>
        /// Noter til salget
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

        /// <summary>
        /// Liste over salgslinjer (produkter i salget)
        /// </summary>
        public ObservableCollection<SaleLine> SaleLines
        {
            get { return _saleLines; }
            set
            {
                _saleLines = value;
                OnPropertyChanged(nameof(SaleLines));
                OnPropertyChanged(nameof(ProductCount));
                CalculateTotal();
            }
        }

        // Beregnet properties for UI visning

        /// <summary>
        /// Formateret dato og tid
        /// </summary>
        public string DatoTidFormatted
        {
            get { return DatoTid.ToString("dd/MM/yyyy HH:mm"); }
        }

        /// <summary>
        /// Formateret total beløb
        /// </summary>
        public string TotalFormatted
        {
            get { return $"{Total:C0}"; }
        }

        /// <summary>
        /// Formateret betalt beløb
        /// </summary>
        public string BetaltBelobFormatted
        {
            get { return $"{BetaltBelob:C0}"; }
        }

        /// <summary>
        /// Formateret byttepenge
        /// </summary>
        public string ByttePengeFormatted
        {
            get { return $"{ByttePenge:C0}"; }
        }

        /// <summary>
        /// Status tekst for salget
        /// </summary>
        public string StatusText
        {
            get
            {
                if (IsCompleted) return "Gennemført";
                if (SaleLines.Count > 0) return "Igangværende";
                return "Nyt salg";
            }
        }

        /// <summary>
        /// Antal produkter i salget
        /// </summary>
        public int ProductCount
        {
            get
            {
                int count = 0;
                foreach (var line in SaleLines)
                {
                    count += line.Antal;
                }
                return count;
            }
        }

        /// <summary>
        /// Display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"Salg {SaleId} - {DatoTidFormatted} - {TotalFormatted}";
            }
        }

        // Metoder til salgslogik

        /// <summary>
        /// Tilføjer en salgslinje til salget
        /// </summary>
        public void AddSaleLine(SaleLine saleLine)
        {
            SaleLines.Add(saleLine);
            CalculateTotal();
        }

        /// <summary>
        /// Fjerner en salgslinje fra salget
        /// </summary>
        public void RemoveSaleLine(SaleLine saleLine)
        {
            SaleLines.Remove(saleLine);
            CalculateTotal();
        }

        /// <summary>
        /// Beregner total baseret på salgslinjer
        /// </summary>
        public void CalculateTotal()
        {
            decimal newTotal = 0;

            foreach (var line in SaleLines)
            {
                newTotal += line.LinjeTotal;
            }

            Total = newTotal;
        }

        /// <summary>
        /// Beregner byttepenge baseret på betalt beløb
        /// </summary>
        private void CalculateByttePenge()
        {
            ByttePenge = BetaltBelob - Total;
        }

        /// <summary>
        /// Gennemfører salget
        /// </summary>
        public bool CompleteSale()
        {
            // Tjek at der er produkter i salget
            if (SaleLines.Count == 0)
                return false;

            // Tjek at der er betalt nok
            if (BetaltBelob < Total)
                return false;

            IsCompleted = true;
            DatoTid = DateTime.Now; // Opdater til gennemførselstidspunkt

            return true;
        }

        /// <summary>
        /// Annullerer salget
        /// </summary>
        public void CancelSale()
        {
            SaleLines.Clear();
            Total = 0;
            BetaltBelob = 0;
            ByttePenge = 0;
            IsCompleted = false;
            Notes = "Annulleret";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}