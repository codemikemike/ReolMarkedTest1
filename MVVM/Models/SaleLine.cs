using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for en salgslinje i et salg
    /// Repræsenterer et enkelt produkt i Ayas køb (f.eks. glasvasen)
    /// </summary>
    public class SaleLine : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _salgsLinjeId;
        private int _saleId;
        private int _labelId;
        private int _antal;
        private decimal _enhedsPris;
        private decimal _linjeTotal;
        private DateTime _addedAt;

        // Navigation properties
        private Sale _sale;
        private Label _label;

        // Konstruktør - opretter en ny salgslinje
        public SaleLine()
        {
            // Sæt standard værdier
            Antal = 1;
            AddedAt = DateTime.Now;
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for salgslinjen
        /// </summary>
        public int SalgsLinjeId
        {
            get { return _salgsLinjeId; }
            set
            {
                _salgsLinjeId = value;
                OnPropertyChanged(nameof(SalgsLinjeId));
            }
        }

        /// <summary>
        /// ID på salget denne linje tilhører
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
        /// ID på label/produktet der sælges
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
        /// Antal af produktet (normalt 1, men kan være flere)
        /// </summary>
        public int Antal
        {
            get { return _antal; }
            set
            {
                _antal = value;
                OnPropertyChanged(nameof(Antal));
                CalculateLinjeTotal();
            }
        }

        /// <summary>
        /// Pris per enhed
        /// </summary>
        public decimal EnhedsPris
        {
            get { return _enhedsPris; }
            set
            {
                _enhedsPris = value;
                OnPropertyChanged(nameof(EnhedsPris));
                OnPropertyChanged(nameof(EnhedsPrisFormatted));
                CalculateLinjeTotal();
            }
        }

        /// <summary>
        /// Total for denne linje (antal * enhedspris)
        /// </summary>
        public decimal LinjeTotal
        {
            get { return _linjeTotal; }
            set
            {
                _linjeTotal = value;
                OnPropertyChanged(nameof(LinjeTotal));
                OnPropertyChanged(nameof(LinjeTotalFormatted));
            }
        }

        /// <summary>
        /// Hvornår produktet blev tilføjet til salget
        /// </summary>
        public DateTime AddedAt
        {
            get { return _addedAt; }
            set
            {
                _addedAt = value;
                OnPropertyChanged(nameof(AddedAt));
            }
        }

        // Navigation properties

        /// <summary>
        /// Salget denne linje tilhører
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
        /// Label/produktet der sælges
        /// </summary>
        public Label Label
        {
            get { return _label; }
            set
            {
                _label = value;
                OnPropertyChanged(nameof(Label));
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(RackNumber));
                OnPropertyChanged(nameof(BarCode));

                // Opdater enhedspris fra label
                if (_label != null)
                {
                    EnhedsPris = _label.ProductPrice;
                }
            }
        }

        // Beregnet properties for UI visning

        /// <summary>
        /// Formateret enhedspris
        /// </summary>
        public string EnhedsPrisFormatted
        {
            get { return $"{EnhedsPris:C0}"; }
        }

        /// <summary>
        /// Formateret linjetotal
        /// </summary>
        public string LinjeTotalFormatted
        {
            get { return $"{LinjeTotal:C0}"; }
        }

        /// <summary>
        /// Produktnavn fra label (hvis tilgængeligt)
        /// </summary>
        public string ProductName
        {
            get
            {
                if (Label != null && Label.BarCode != null)
                {
                    // Udled produktnavn fra stregkode
                    // Format: "REOL07-VASE-50KR-001"
                    string[] parts = Label.BarCode.Split('-');
                    if (parts.Length >= 2)
                    {
                        return parts[1]; // "VASE" delen
                    }
                }
                return "Ukendt produkt";
            }
        }

        /// <summary>
        /// Reolnummer fra label
        /// </summary>
        public int RackNumber
        {
            get { return Label?.RackId ?? 0; }
        }

        /// <summary>
        /// Stregkode fra label
        /// </summary>
        public string BarCode
        {
            get { return Label?.BarCode ?? ""; }
        }

        /// <summary>
        /// Display tekst til UI
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (Antal > 1)
                    return $"{ProductName} - {EnhedsPrisFormatted} x {Antal} = {LinjeTotalFormatted}";
                else
                    return $"{ProductName} - {LinjeTotalFormatted}";
            }
        }

        /// <summary>
        /// Detaljeret display med stregkode
        /// </summary>
        public string DetailedDisplayText
        {
            get
            {
                return $"{BarCode} - {ProductName} - {LinjeTotalFormatted} (Reol {RackNumber})";
            }
        }

        // Metoder

        /// <summary>
        /// Beregner linjetotal baseret på antal og enhedspris
        /// </summary>
        private void CalculateLinjeTotal()
        {
            LinjeTotal = Antal * EnhedsPris;
        }

        /// <summary>
        /// Opretter en salgslinje fra et label
        /// </summary>
        public static SaleLine CreateFromLabel(Label label, int antal = 1)
        {
            if (label == null)
                return null;

            var saleLine = new SaleLine
            {
                LabelId = label.LabelId,
                Label = label,
                Antal = antal,
                EnhedsPris = label.ProductPrice,
                AddedAt = DateTime.Now
            };

            return saleLine;
        }

        /// <summary>
        /// Validerer at salgslinjen er gyldig
        /// </summary>
        public bool IsValid()
        {
            return LabelId > 0 &&
                   Antal > 0 &&
                   EnhedsPris > 0 &&
                   Label != null &&
                   !Label.IsSold &&
                   !Label.IsVoid;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}