using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for anmodning om label oprettelse
    /// Bruges når kunder vil oprette stregkoder til deres produkter
    /// </summary>
    public class LabelRequest : INotifyPropertyChanged
    {
        // Private felter
        private string _name = "";
        private decimal _price;
        private int _quantity = 1;

        /// <summary>
        /// Navn på produktet der skal have stregkode
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Pris for produktet i kroner
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
        /// Antal stregkoder der skal oprettes for dette produkt
        /// </summary>
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(TotalPrice));
                OnPropertyChanged(nameof(TotalPriceFormatted));
            }
        }

        // Beregnet properties

        /// <summary>
        /// Formateret pris
        /// </summary>
        public string PriceFormatted
        {
            get { return $"{Price:C0}"; }
        }

        /// <summary>
        /// Total pris for alle enheder
        /// </summary>
        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }

        /// <summary>
        /// Formateret total pris
        /// </summary>
        public string TotalPriceFormatted
        {
            get { return $"{TotalPrice:C0}"; }
        }

        /// <summary>
        /// Display tekst til UI
        /// </summary>
        public string DisplayText
        {
            get
            {
                if (Quantity > 1)
                    return $"{Name} - {PriceFormatted} x {Quantity} = {TotalPriceFormatted}";
                else
                    return $"{Name} - {PriceFormatted}";
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