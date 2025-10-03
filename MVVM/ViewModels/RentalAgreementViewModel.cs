using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel der wrapper RentalAgreement model
    /// </summary>
    public class RentalAgreementViewModel : ViewModelBase
    {
        private readonly RentalAgreement _model;

        public RentalAgreementViewModel(RentalAgreement model)
        {
            _model = model;
        }

        public int AgreementId => _model.AgreementId;
        public int CustomerId => _model.CustomerId;
        public int RackId => _model.RackId;

        public decimal MonthlyRent
        {
            get => _model.MonthlyRent;
            set
            {
                if (_model.MonthlyRent != value)
                {
                    _model.MonthlyRent = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MonthlyRentFormatted));
                }
            }
        }

        public RentalStatus Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        // UI-specific properties
        public string StartDateFormatted => _model.StartDate.ToString("dd/MM/yyyy");
        public string MonthlyRentFormatted => $"{MonthlyRent:C0}";
        public bool IsActive => Status == RentalStatus.Active;
        public string StatusText => Status.ToString();
        public string CustomerName => _model.Customer?.Name ?? "Ukendt";
        public string RackNumber => _model.Rack?.RackNumber.ToString() ?? "Ukendt";
        public string DisplayText => $"Aftale {AgreementId} - {CustomerName} - Reol {RackNumber}";

        public RentalAgreement Model => _model;
    }
}