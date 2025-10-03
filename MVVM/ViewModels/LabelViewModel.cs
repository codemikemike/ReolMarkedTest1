using System;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    public class LabelViewModel : ViewModelBase
    {
        private readonly Label _model;

        public LabelViewModel(Label model)
        {
            _model = model;
        }

        public int LabelId => _model.LabelId;

        public decimal ProductPrice
        {
            get => _model.ProductPrice;
            set
            {
                if (_model.ProductPrice != value)
                {
                    _model.ProductPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProductPriceFormatted));
                }
            }
        }

        public int RackId => _model.RackId;

        public string BarCode
        {
            get => _model.BarCode;
            set
            {
                if (_model.BarCode != value)
                {
                    _model.BarCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? SoldDate
        {
            get => _model.SoldDate;
            set
            {
                if (_model.SoldDate != value)
                {
                    _model.SoldDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSold));
                    OnPropertyChanged(nameof(SoldDateFormatted));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public bool IsVoid
        {
            get => _model.IsVoid;
            set
            {
                if (_model.IsVoid != value)
                {
                    _model.IsVoid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        // UI-specific properties
        public string ProductPriceFormatted => $"{ProductPrice:C0}";
        public string SoldDateFormatted => SoldDate?.ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
        public bool IsSold => SoldDate.HasValue;

        public string StatusText
        {
            get
            {
                if (IsVoid) return "Annulleret";
                if (IsSold) return "Solgt";
                return "Aktiv";
            }
        }

        public string CreatedAtFormatted => _model.CreatedAt.ToString("dd/MM/yyyy HH:mm");
        public string DisplayText => $"{BarCode} - {ProductPriceFormatted}";

        public Label Model => _model;
    }
}