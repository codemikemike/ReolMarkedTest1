using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for et label/stregkode i reolmarkedet
    /// Indeholder labelets grundlæggende oplysninger og stregkode
    /// </summary>
    public class Label : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _labelId;
        private decimal _productPrice;
        private int _rackId;
        private string _barCode = "";
        private DateTime? _sold;
        private DateTime _createdAt;
        private bool _isVoid;

        // Navigation properties
        private Customer _customer;
        private Rack _rack;

        // Konstruktør - opretter et nyt label
        public Label()
        {
            // Sæt standard værdier
            CreatedAt = DateTime.Now;
            IsVoid = false;
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for labelet i databasen
        /// </summary>
        public int LabelId
        {
            get { return _labelId; }
            set
            {
                _labelId = value;
                OnPropertyChanged(nameof(LabelId));
            }
        }

        /// <summary>
        /// Produktets pris i kroner
        /// </summary>
        public decimal ProductPrice
        {
            get { return _productPrice; }
            set
            {
                _productPrice = value;
                OnPropertyChanged(nameof(ProductPrice));
                OnPropertyChanged(nameof(ProductPriceFormatted));
                GenerateBarCode(); // Generer ny stregkode når pris ændres
            }
        }

        /// <summary>
        /// Hvilken reol produktet skal stå på
        /// </summary>
        public int RackId
        {
            get { return _rackId; }
            set
            {
                _rackId = value;
                OnPropertyChanged(nameof(RackId));
                GenerateBarCode(); // Generer ny stregkode når reol ændres
            }
        }

        /// <summary>
        /// Stregkode knyttet til reolen (f.eks. "REOL07-50KR-001")
        /// </summary>
        public string BarCode
        {
            get { return _barCode; }
            set
            {
                _barCode = value;
                OnPropertyChanged(nameof(BarCode));
            }
        }

        /// <summary>
        /// Hvornår produktet blev solgt (null hvis ikke solgt)
        /// </summary>
        public DateTime? Sold
        {
            get { return _sold; }
            set
            {
                _sold = value;
                OnPropertyChanged(nameof(Sold));
                OnPropertyChanged(nameof(SoldFormatted));
                OnPropertyChanged(nameof(IsSold));
            }
        }

        /// <summary>
        /// Hvornår labelet blev oprettet
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
        /// Om labelet er annulleret
        /// </summary>
        public bool IsVoid
        {
            get { return _isVoid; }
            set
            {
                _isVoid = value;
                OnPropertyChanged(nameof(IsVoid));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        // Navigation properties
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
            }
        }

        /// <summary>
        /// Reolen produktet skal stå på
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
        /// Formateret pris
        /// </summary>
        public string ProductPriceFormatted
        {
            get { return $"{ProductPrice:C0}"; } // Dansk valuta format
        }

        /// <summary>
        /// Formateret solgt dato
        /// </summary>
        public string SoldFormatted
        {
            get
            {
                if (Sold.HasValue)
                    return Sold.Value.ToString("dd/MM/yyyy HH:mm");
                return "";
            }
        }

        /// <summary>
        /// Om produktet er solgt
        /// </summary>
        public bool IsSold
        {
            get { return Sold.HasValue; }
        }

        /// <summary>
        /// Status tekst for labelet
        /// </summary>
        public string StatusText
        {
            get
            {
                if (IsVoid) return "Annulleret";
                if (IsSold) return "Solgt";
                return "Aktiv";
            }
        }

        /// <summary>
        /// Display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"{BarCode} - {ProductPriceFormatted} (Reol {RackId})";
            }
        }

        /// <summary>
        /// Oprettet dato formateret
        /// </summary>
        public string CreatedAtFormatted
        {
            get { return CreatedAt.ToString("dd/MM/yyyy HH:mm"); }
        }

        // Metoder som specificeret i DCD

        /// <summary>
        /// Opretter et nyt label med specificerede værdier
        /// </summary>
        public static Label CreateLabel(int rackId, decimal price, string barCode, string labelName = "")
        {
            var label = new Label
            {
                RackId = rackId,
                ProductPrice = price,
                BarCode = barCode,
                CreatedAt = DateTime.Now
            };

            return label;
        }

        /// <summary>
        /// Genererer et enkelt label og returnerer labelId
        /// </summary>
        public static int GenerateLabel(int rackId, decimal price)
        {
            var label = new Label
            {
                RackId = rackId,
                ProductPrice = price,
                CreatedAt = DateTime.Now
            };

            label.GenerateBarCode();

            // I en rigtig implementering ville vi gemme i database her
            // og returnere det rigtige ID fra databasen
            return DateTime.Now.Millisecond; // Temp ID
        }

        /// <summary>
        /// Genererer flere labels og returnerer en liste af labelId'er
        /// </summary>
        public static IEnumerable<int> GenerateLabels(int rackId, decimal price, int count)
        {
            var labelIds = new List<int>();

            for (int i = 0; i < count; i++)
            {
                var labelId = GenerateLabel(rackId, price);
                labelIds.Add(labelId);

                // Lille pause for at undgå samme millisekund
                System.Threading.Thread.Sleep(1);
            }

            return labelIds;
        }

        /// <summary>
        /// Annullerer et label baseret på labelId
        /// </summary>
        public void VoidLabel(int labelId)
        {
            if (this.LabelId == labelId)
            {
                IsVoid = true;
            }
        }

        // Private metoder

        /// <summary>
        /// Genererer en stregkode baseret på reol og pris
        /// Format: "REOL07-50KR-001"
        /// </summary>
        public void GenerateBarCode()
        {
            if (RackId > 0 && ProductPrice > 0)
            {
                // Generer unikt nummer baseret på LabelId og tidspunkt
                int uniqueNumber = LabelId > 0 ? LabelId : DateTime.Now.Millisecond + DateTime.Now.Second * 1000;

                // Generer stregkode
                string newBarCode = $"REOL{RackId:D2}-{ProductPrice:F0}KR-{uniqueNumber:D3}";

                // Undgå uendelig loop ved at tjekke om stregkoden faktisk ændres
                if (BarCode != newBarCode)
                {
                    BarCode = newBarCode;
                }
            }
        }

        /// <summary>
        /// Markerer labelet som solgt
        /// </summary>
        public void MarkAsSold()
        {
            if (!IsVoid)
            {
                Sold = DateTime.Now;
            }
        }

        /// <summary>
        /// Parser en stregkode og returnerer reolnummer og pris
        /// </summary>
        public static bool ParseBarCode(string barCode, out int rackId, out decimal price)
        {
            rackId = 0;
            price = 0;

            if (string.IsNullOrEmpty(barCode))
                return false;

            try
            {
                // Format: "REOL07-50KR-001"
                string[] parts = barCode.Split('-');
                if (parts.Length >= 2)
                {
                    // Parse reol nummer fra "REOL07"
                    string rackPart = parts[0];
                    if (rackPart.StartsWith("REOL"))
                    {
                        string rackNumberText = rackPart.Substring(4); // Fjern "REOL"
                        if (int.TryParse(rackNumberText, out rackId))
                        {
                            // Parse pris fra "50KR"
                            string pricePart = parts[1];
                            if (pricePart.EndsWith("KR"))
                            {
                                string priceText = pricePart.Substring(0, pricePart.Length - 2); // Fjern "KR"
                                if (decimal.TryParse(priceText, out price))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorer parsing fejl
            }

            return false;
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