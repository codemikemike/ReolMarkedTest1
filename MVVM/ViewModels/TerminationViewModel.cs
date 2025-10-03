using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    public class TerminationViewModel : ViewModelBase
    {
        private readonly TerminationService _terminationService;
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;

        private string _customerPhone = string.Empty;
        private CustomerViewModel _selectedCustomer;
        private ObservableCollection<RackViewModel> _customerRacks;
        private int _selectedRackNumber;
        private DateTime _desiredTerminationDate;
        private string _terminationReason = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _useCustomDate;

        private ObservableCollection<RackTerminationViewModel> _activeTerminations;
        private ObservableCollection<RackTerminationViewModel> _customerTerminations;
        private RackTerminationViewModel _selectedTermination;

        public TerminationViewModel()
        {
            // Hent services fra ServiceLocator
            _terminationService = ServiceLocator.TerminationService;
            _customerService = ServiceLocator.CustomerService;
            _rentalService = ServiceLocator.RentalService;

            _customerRacks = new ObservableCollection<RackViewModel>();
            _activeTerminations = new ObservableCollection<RackTerminationViewModel>();
            _customerTerminations = new ObservableCollection<RackTerminationViewModel>();

            DesiredTerminationDate = DateTime.Now.Date.AddMonths(1);
            StatusMessage = "Find kunde for at registrere opsigelse";

            CreateCommands();
            LoadTerminationData();
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                if (SetProperty(ref _customerPhone, value))
                    OnPropertyChanged(nameof(CanFindCustomer));
            }
        }

        public CustomerViewModel SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(IsCustomerSelected));
                    if (value != null)
                    {
                        LoadCustomerRacks();
                        LoadCustomerTerminations();
                        StatusMessage = $"Kunde fundet: {value.Name}";
                    }
                    else
                    {
                        CustomerRacks.Clear();
                        CustomerTerminations.Clear();
                    }
                }
            }
        }

        public ObservableCollection<RackViewModel> CustomerRacks
        {
            get => _customerRacks;
            set => SetProperty(ref _customerRacks, value);
        }

        public int SelectedRackNumber
        {
            get => _selectedRackNumber;
            set
            {
                if (SetProperty(ref _selectedRackNumber, value))
                {
                    OnPropertyChanged(nameof(IsRackSelected));
                    OnPropertyChanged(nameof(CanCreateTermination));
                    if (value > 0 && !UseCustomDate)
                        CalculateAutomaticDate();
                }
            }
        }

        public DateTime DesiredTerminationDate
        {
            get => _desiredTerminationDate;
            set
            {
                if (SetProperty(ref _desiredTerminationDate, value))
                    OnPropertyChanged(nameof(DesiredTerminationDateFormatted));
            }
        }

        public bool UseCustomDate
        {
            get => _useCustomDate;
            set
            {
                if (SetProperty(ref _useCustomDate, value))
                    OnPropertyChanged(nameof(UseAutomaticDate));
            }
        }

        public string TerminationReason
        {
            get => _terminationReason;
            set => SetProperty(ref _terminationReason, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<RackTerminationViewModel> ActiveTerminations
        {
            get => _activeTerminations;
            set => SetProperty(ref _activeTerminations, value);
        }

        public ObservableCollection<RackTerminationViewModel> CustomerTerminations
        {
            get => _customerTerminations;
            set => SetProperty(ref _customerTerminations, value);
        }

        public RackTerminationViewModel SelectedTermination
        {
            get => _selectedTermination;
            set
            {
                if (SetProperty(ref _selectedTermination, value))
                    OnPropertyChanged(nameof(IsTerminationSelected));
            }
        }

        public bool CanFindCustomer => !string.IsNullOrEmpty(CustomerPhone);
        public bool IsCustomerSelected => SelectedCustomer != null;
        public bool HasCustomerRacks => CustomerRacks != null && CustomerRacks.Any();
        public bool IsRackSelected => SelectedRackNumber > 0;
        public bool CanCreateTermination => IsCustomerSelected && IsRackSelected;
        public bool UseAutomaticDate => !UseCustomDate;
        public bool HasActiveTerminations => ActiveTerminations != null && ActiveTerminations.Any();
        public bool IsTerminationSelected => SelectedTermination != null;
        public string DesiredTerminationDateFormatted => DesiredTerminationDate.ToString("dd/MM/yyyy");

        public RelayCommand FindCustomerCommand { get; private set; }
        public RelayCommand CreateTerminationCommand { get; private set; }
        public RelayCommand CancelTerminationCommand { get; private set; }
        public RelayCommand ProcessEffectiveTerminationsCommand { get; private set; }
        public RelayCommand RefreshDataCommand { get; private set; }
        public RelayCommand ClearSelectionCommand { get; private set; }

        private void LoadCustomerRacks()
        {
            if (SelectedCustomer != null)
            {
                var racks = _rentalService.GetRacksForCustomer(SelectedCustomer.CustomerId);
                CustomerRacks = new ObservableCollection<RackViewModel>(
                    racks.Select(r => new RackViewModel(r)));
            }
        }

        private void LoadCustomerTerminations()
        {
            if (SelectedCustomer != null)
            {
                var terminations = _terminationService.GetTerminationsForCustomer(SelectedCustomer.CustomerId);
                CustomerTerminations = new ObservableCollection<RackTerminationViewModel>(
                    terminations.Select(t => new RackTerminationViewModel(t)));
            }
        }

        private void LoadTerminationData()
        {
            var activeTerminations = _terminationService.GetActiveTerminations();
            ActiveTerminations = new ObservableCollection<RackTerminationViewModel>(
                activeTerminations.Select(t => new RackTerminationViewModel(t)));
        }

        private void CalculateAutomaticDate()
        {
            if (!UseCustomDate)
            {
                DesiredTerminationDate = _terminationService.CalculateEffectiveDate(DateTime.Now.Date);
            }
        }

        private void CreateCommands()
        {
            FindCustomerCommand = new RelayCommand(FindCustomer, _ => CanFindCustomer);
            CreateTerminationCommand = new RelayCommand(CreateTermination, _ => CanCreateTermination);
            CancelTerminationCommand = new RelayCommand(CancelTermination);
            ProcessEffectiveTerminationsCommand = new RelayCommand(ProcessEffectiveTerminations);
            RefreshDataCommand = new RelayCommand(RefreshData);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
        }

        private void FindCustomer(object parameter)
        {
            var customer = _customerService.FindCustomerByPhone(CustomerPhone);
            if (customer != null)
            {
                SelectedCustomer = new CustomerViewModel(customer);
            }
            else
            {
                MessageBox.Show("Kunde ikke fundet.", "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Kunde ikke fundet";
            }
        }

        private void CreateTermination(object parameter)
        {
            if (SelectedCustomer == null || SelectedRackNumber <= 0)
                return;

            DateTime? desiredDate = UseCustomDate ? DesiredTerminationDate : (DateTime?)null;

            var result = _terminationService.CreateTermination(
                SelectedCustomer.CustomerId,
                SelectedRackNumber,
                desiredDate,
                TerminationReason);

            if (result.Success)
            {
                MessageBox.Show(
                    $"Opsigelse oprettet!\n\nKunde: {SelectedCustomer.Name}\nReol: {SelectedRackNumber}\nTræder i kraft: {result.Termination.EffectiveDate:dd/MM/yyyy}",
                    "Opsigelse oprettet",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StatusMessage = result.Message;
                LoadTerminationData();
                LoadCustomerTerminations();
                SelectedRackNumber = 0;
                TerminationReason = string.Empty;
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelTermination(object parameter)
        {
            if (parameter is RackTerminationViewModel terminationVM)
            {
                var dialogResult = MessageBox.Show(
                    $"Annuller opsigelse for {terminationVM.CustomerName} - Reol {terminationVM.RackNumber}?",
                    "Annuller opsigelse",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    var result = _terminationService.CancelTermination(terminationVM.TerminationId);
                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTerminationData();
                        LoadCustomerTerminations();
                    }
                    else
                    {
                        MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ProcessEffectiveTerminations(object parameter)
        {
            var result = _terminationService.ProcessEffectiveTerminations();

            if (result.Success && result.ProcessedTerminations.Any())
            {
                MessageBox.Show(
                    $"Behandlet {result.ProcessedTerminations.Count} opsigelser",
                    "Opsigelser behandlet",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                LoadTerminationData();
            }
            else
            {
                MessageBox.Show("Ingen opsigelser at behandle", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshData(object parameter)
        {
            LoadTerminationData();
            if (SelectedCustomer != null)
                LoadCustomerTerminations();
            StatusMessage = "Data genindlæst";
        }

        private void ClearSelection(object parameter)
        {
            SelectedCustomer = null;
            CustomerPhone = string.Empty;
            SelectedRackNumber = 0;
            TerminationReason = string.Empty;
            UseCustomDate = false;
            StatusMessage = "Find kunde for at registrere opsigelse";
        }
    }
}