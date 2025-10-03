using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    public class SaleLineViewModel : ViewModelBase
    {
        private readonly SaleLine _model;

        public SaleLineViewModel(SaleLine model)
        {
            _model = model;
        }

        public int SaleLineId => _model.SaleLineId;
        public int SaleId => _model.SaleId;
        public int LabelId => _model.LabelId;

        public int Quantity
        {
            get => _model.Quantity;
            set
            {
                if (_model.Quantity != value)
                {
                    _model.Quantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public decimal UnitPrice
        {
            get => _model.UnitPrice;
            set
            {
                if (_model.UnitPrice != value)
                {
                    _model.UnitPrice = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LineTotal));
                }
            }
        }

        public decimal LineTotal => _model.LineTotal;

        public string ProductName => _model.Label?.BarCode ?? "Ukendt";
        public string LineTotalFormatted => $"{LineTotal:C0}";

        public SaleLine Model => _model;
    }
}