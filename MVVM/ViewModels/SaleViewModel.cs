using System.Collections.ObjectModel;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel der wrapper Sale model
    /// </summary>
    public class SaleViewModel : ViewModelBase
    {
        private readonly Sale _model;
        private ObservableCollection<SaleLineViewModel> _saleLines;

        public SaleViewModel(Sale model)
        {
            _model = model;
            _saleLines = new ObservableCollection<SaleLineViewModel>(
                model.SaleLines.Select(sl => new SaleLineViewModel(sl)));
        }

        public int SaleId => _model.SaleId;

        public decimal Total
        {
            get => _model.Total;
            set
            {
                if (_model.Total != value)
                {
                    _model.Total = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TotalFormatted));
                }
            }
        }

        public decimal AmountPaid
        {
            get => _model.AmountPaid;
            set
            {
                if (_model.AmountPaid != value)
                {
                    _model.AmountPaid = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AmountPaidFormatted));
                }
            }
        }

        public decimal ChangeGiven
        {
            get => _model.ChangeGiven;
            set
            {
                if (_model.ChangeGiven != value)
                {
                    _model.ChangeGiven = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ChangeGivenFormatted));
                }
            }
        }

        public PaymentMethod PaymentMethod
        {
            get => _model.PaymentMethod;
            set
            {
                if (_model.PaymentMethod != value)
                {
                    _model.PaymentMethod = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PaymentMethodText));
                }
            }
        }

        public bool IsCompleted => _model.IsCompleted;

        public ObservableCollection<SaleLineViewModel> SaleLines
        {
            get => _saleLines;
            set => SetProperty(ref _saleLines, value);
        }

        // UI-specific properties
        public string SaleDateTimeFormatted => _model.SaleDateTime.ToString("dd/MM/yyyy HH:mm");
        public string TotalFormatted => $"{Total:C0}";
        public string AmountPaidFormatted => $"{AmountPaid:C0}";
        public string ChangeGivenFormatted => $"{ChangeGiven:C0}";
        public string PaymentMethodText => PaymentMethod.ToString();
        public int ProductCount => SaleLines.Sum(sl => sl.Quantity);
        public string StatusText => IsCompleted ? "Gennemført" : "Igangværende";

        public Sale Model => _model;
    }
}