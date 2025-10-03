// ViewModels/RackViewModel.cs
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel der wrapper Rack model
    /// </summary>
    public class RackViewModel : ViewModelBase
    {
        private readonly Rack _model;

        public RackViewModel(Rack model)
        {
            _model = model;
        }

        public int RackId => _model.RackId;

        public int RackNumber
        {
            get => _model.RackNumber;
            set
            {
                if (_model.RackNumber != value)
                {
                    _model.RackNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasHangerBar
        {
            get => _model.HasHangerBar;
            set
            {
                if (_model.HasHangerBar != value)
                {
                    _model.HasHangerBar = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RackType));
                }
            }
        }

        public int AmountShelves
        {
            get => _model.AmountShelves;
            set
            {
                if (_model.AmountShelves != value)
                {
                    _model.AmountShelves = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RackType));
                }
            }
        }

        public string Location
        {
            get => _model.Location;
            set
            {
                if (_model.Location != value)
                {
                    _model.Location = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAvailable
        {
            get => _model.IsAvailable;
            set
            {
                if (_model.IsAvailable != value)
                {
                    _model.IsAvailable = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(StatusText));
                }
            }
        }

        public string Description
        {
            get => _model.Description;
            set
            {
                if (_model.Description != value)
                {
                    _model.Description = value;
                    OnPropertyChanged();
                }
            }
        }

        // UI-specific properties
        public string RackType
        {
            get
            {
                if (HasHangerBar)
                    return $"{AmountShelves} hylder + bøjlestang";
                return $"{AmountShelves} hylder";
            }
        }

        public string StatusText => IsAvailable ? "Ledig" : "Optaget";

        public string DisplayText => $"Reol {RackNumber} - {Location}";

        public Rack Model => _model;
    }
}