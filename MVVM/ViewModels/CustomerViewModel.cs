using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel der wrapper Customer model
    /// </summary>
    public class CustomerViewModel : ViewModelBase
    {
        private readonly Customer _model;

        public CustomerViewModel(Customer model)
        {
            _model = model;
        }

        // Wrapper model properties med INotifyPropertyChanged
        public int CustomerId => _model.CustomerId;

        public string Name
        {
            get => _model.Name;
            set
            {
                if (_model.Name != value)
                {
                    _model.Name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText)); // RETTET fra DisplayName
                }
            }
        }

        public string Phone
        {
            get => _model.Phone;
            set
            {
                if (_model.Phone != value)
                {
                    _model.Phone = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText)); // RETTET fra DisplayName
                }
            }
        }

        public string Email
        {
            get => _model.Email;
            set
            {
                if (_model.Email != value)
                {
                    _model.Email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Address
        {
            get => _model.Address;
            set
            {
                if (_model.Address != value)
                {
                    _model.Address = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _model.IsActive;
            set
            {
                if (_model.IsActive != value)
                {
                    _model.IsActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        // UI-specific properties
        public string DisplayText => $"{Name} - {Phone}"; // RETTET fra DisplayName
        public string StatusText => IsActive ? "Aktiv" : "Inaktiv";
        public string CreatedAtFormatted => _model.CreatedAt.ToString("dd/MM/yyyy");

        // Reference til den underliggende model
        public Customer Model => _model;
    }
}