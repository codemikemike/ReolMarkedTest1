using ReolMarked.MVVM.Models;
using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for reol salg
    /// Forbinder salg til specifikke reoler og deres ejere
    /// Som når Jonas noterer salg på reolernes kort
    /// </summary>
    public class RackSale : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _reolSalgId;
        private int _saleId;
        private int _rackNumber;
        private int _customerId;
        private DateTime _dato;
        private decimal _belob;
        private string _productInfo = "";
        private string _notes = "";

        // Navigation properties
        private Sale _sale;
        private Rack _rack;
        private Customer _customer;

        // Konstruktør - opretter et nyt reol salg
        public RackSale()
        {
            // Sæt standard værdier
            Dato = DateTime.Now;
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for reol salget
        /// </summary>
        public int ReolSalgId
        {
            get { return _reolSalgId; }
            set
            {
                _reolSalgId = value;
                OnPropertyChanged(nameof(ReolSalgId));
            }
        }

        /// <summary>
        /// ID på salget dette reol salg tilhører
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
        /// Reolnummer hvor produktet blev solgt fra
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
        /// ID på kunden der ejer reolen
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
        /// Dato for salget
        /// </summary>
        public DateTime Dato
        {
            get { return _dato; }
            set
            {
                _dato = value;
                OnPropertyChanged(nameof(Dato));
                OnPropertyChanged(nameof(DatoFormatted));
            }
        }

        /// <summary>
        /// Beløb der er solgt for fra denne reol
        /// </summary>
        public decimal Belob
        {
            get { return _belob; }
            set
            {
                _belob = value;
                OnPropertyChanged(nameof(Belob));
                OnPropertyChanged(nameof(BelobFormatted));
            }
        }

        /// <summary>
        /// Information om det solgte produkt
        /// </summary>
        public string ProductInfo
        {
            get { return _productInfo; }
            set
            {
                _productInfo = value;
                OnPropertyChanged(nameof(ProductInfo));
            }
        }

        /// <summary>
        /// Noter til reol salget
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
        /// Salget dette reol salg tilhører
        /// </summary>
        public Sale Sale
        {
            get { return _sale; }
            set
            {
                _sale = value;
                OnPropertyChanged(nameof(Sale));
            }
        }

        /// <summary>
        /// Reolen produktet blev solgt fra
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

        /// <summary>
        /// Kunden der ejer reolen
        /// </summary>
        public Customer Customer
        {
            get { return _customer; }
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        // Beregnet properties for UI visning

        /// <summary>
        /// Formateret dato
        /// </summary>
        public string DatoFormatted
        {
            get { return Dato.ToString("dd/MM/yyyy"); }
        }

        /// <summary>
        /// Formateret beløb
        /// </summary>
        public string BelobFormatted
        {
            get { return $"{Belob:C0}"; }
        }

        /// <summary>
        /// Kundens navn
        /// </summary>
        public string CustomerName
        {
            get { return Customer?.CustomerName ?? "Ukendt kunde"; }
        }

        /// <summary>
        /// Display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"Reol {RackNumber} - {BelobFormatted} - {CustomerName}";
            }
        }

        /// <summary>
        /// Detaljeret display tekst
        /// </summary>
        public string DetailedDisplayText
        {
            get
            {
                return $"{DatoFormatted}: {ProductInfo} - {BelobFormatted} (Reol {RackNumber})";
            }
        }

        // Metoder

        /// <summary>
        /// Opretter et reol salg fra en salgslinje
        /// </summary>
        public static RackSale CreateFromSaleLine(SaleLine saleLine)
        {
            if (saleLine?.Label == null)
                return null;

            var rackSale = new RackSale
            {
                SaleId = saleLine.SaleId,
                RackNumber = saleLine.Label.RackId,
                CustomerId = saleLine.Label.Customer?.CustomerId ?? 0,
                Dato = DateTime.Now,
                Belob = saleLine.LinjeTotal,
                ProductInfo = $"{saleLine.ProductName} ({saleLine.BarCode})",
                Notes = $"Salg via scanner - {saleLine.Antal} stk"
            };

            // Navigation properties
            rackSale.Sale = saleLine.Sale;
            rackSale.Rack = saleLine.Label.Rack;
            rackSale.Customer = saleLine.Label.Customer;

            return rackSale;
        }

        /// <summary>
        /// Validerer at reol salget er gyldigt
        /// </summary>
        public bool IsValid()
        {
            return SaleId > 0 &&
                   RackNumber > 0 &&
                   CustomerId > 0 &&
                   Belob > 0;
        }

        /// <summary>
        /// Beregner kommission til butikken (f.eks. 10%)
        /// </summary>
        public decimal CalculateStoreCommission(decimal commissionRate = 0.10m)
        {
            return Belob * commissionRate;
        }

        /// <summary>
        /// Beregner udbetalingsbeløb til reol ejer (efter kommission)
        /// </summary>
        public decimal CalculateOwnerPayout(decimal commissionRate = 0.10m)
        {
            return Belob - CalculateStoreCommission(commissionRate);
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}