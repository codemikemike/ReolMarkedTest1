using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.ViewModels.Base;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ReolMarked.MVVM.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly RentalService _rentalService;
        private readonly CustomerService _customerService;
        private readonly IRackRepository _rackRepository;
        private readonly IWindowService _windowService;

        // Observable collections
        private ObservableCollection<RackViewModel> _availableRacks;
        private ObservableCollection<CustomerViewModel> _customers;
        private ObservableCollection<RackViewModel> _customerRacks;
        private ObservableCollection<RackViewModel> _neighborRacks;

        // Selected items
        private RackViewModel _selectedRack;
        private CustomerViewModel _selectedCustomer;
        private RackViewModel _selectedCustomerRack;

        // Form fields
        private string _newCustomerName = string.Empty;
        private string _newCustomerPhone = string.Empty;
        private string _newCustomerEmail = string.Empty;
        private string _newCustomerAddress = string.Empty;
        private string _statusMessage = string.Empty;

        public MainViewModel()
        {
            // Hent services fra ServiceLocator
            _rentalService = ServiceLocator.RentalService;
            _customerService = ServiceLocator.CustomerService;
            _rackRepository = ServiceLocator.RackRepository;
            _windowService = ServiceLocator.WindowService;

            // Debug: Tjek om repositories er oprettet
            System.Diagnostics.Debug.WriteLine($"RackRepository exists: {_rackRepository != null}");
            System.Diagnostics.Debug.WriteLine($"CustomerService exists: {_customerService != null}");

            // Initialiser collections
            _availableRacks = new ObservableCollection<RackViewModel>();
            _customers = new ObservableCollection<CustomerViewModel>();
            _customerRacks = new ObservableCollection<RackViewModel>();
            _neighborRacks = new ObservableCollection<RackViewModel>();

            // Opret commands
            CreateCommands();

            // Load data fra database
            LoadCustomersFromDatabase();
            LoadRacksFromDatabase();

            // Debug: Tjek collections
            System.Diagnostics.Debug.WriteLine($"AvailableRacks in collection: {AvailableRacks.Count}");
            System.Diagnostics.Debug.WriteLine($"Customers in collection: {Customers.Count}");

            StatusMessage = "Klar til reol administration - Database tilsluttet";
        }

        // Properties
        public ObservableCollection<RackViewModel> AvailableRacks
        {
            get => _availableRacks;
            set => SetProperty(ref _availableRacks, value);
        }

        public ObservableCollection<CustomerViewModel> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        public ObservableCollection<RackViewModel> CustomerRacks
        {
            get => _customerRacks;
            set => SetProperty(ref _customerRacks, value);
        }

        public ObservableCollection<RackViewModel> NeighborRacks
        {
            get => _neighborRacks;
            set => SetProperty(ref _neighborRacks, value);
        }

        public RackViewModel SelectedRack
        {
            get => _selectedRack;
            set
            {
                if (SetProperty(ref _selectedRack, value))
                {
                    OnPropertyChanged(nameof(IsRackSelected));
                    OnPropertyChanged(nameof(CanCreateContract));
                    if (value != null)
                        StatusMessage = $"Valgt reol {value.RackNumber} - {value.Location}";
                }
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
                    OnPropertyChanged(nameof(CanCreateContract));

                    if (value != null)
                    {
                        LoadCustomerRacks();
                        StatusMessage = $"Valgt kunde: {value.Name}";
                    }
                    else
                    {
                        CustomerRacks.Clear();
                        NeighborRacks.Clear();
                    }
                }
            }
        }

        public RackViewModel SelectedCustomerRack
        {
            get => _selectedCustomerRack;
            set
            {
                if (SetProperty(ref _selectedCustomerRack, value))
                {
                    OnPropertyChanged(nameof(IsCustomerRackSelected));

                    if (value != null && SelectedCustomer != null)
                    {
                        LoadNeighborRacks();
                        StatusMessage = $"Viser nabo-reoler til reol {value.RackNumber}";
                    }
                    else
                    {
                        NeighborRacks.Clear();
                    }
                }
            }
        }

        public string NewCustomerName
        {
            get => _newCustomerName;
            set
            {
                if (SetProperty(ref _newCustomerName, value))
                    OnPropertyChanged(nameof(CanCreateCustomer));
            }
        }

        public string NewCustomerPhone
        {
            get => _newCustomerPhone;
            set
            {
                if (SetProperty(ref _newCustomerPhone, value))
                    OnPropertyChanged(nameof(CanCreateCustomer));
            }
        }

        public string NewCustomerEmail
        {
            get => _newCustomerEmail;
            set => SetProperty(ref _newCustomerEmail, value);
        }

        public string NewCustomerAddress
        {
            get => _newCustomerAddress;
            set => SetProperty(ref _newCustomerAddress, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Computed properties
        public bool IsRackSelected => SelectedRack != null;
        public bool IsCustomerSelected => SelectedCustomer != null;
        public bool IsCustomerRackSelected => SelectedCustomerRack != null;

        public bool CanCreateCustomer =>
            !string.IsNullOrEmpty(NewCustomerName) &&
            !string.IsNullOrEmpty(NewCustomerPhone);

        public bool CanCreateContract =>
            IsCustomerSelected &&
            (IsRackSelected || (NeighborRacks != null && NeighborRacks.Any()));

        // Commands
        public RelayCommand ShowAvailableRacksCommand { get; private set; }
        public RelayCommand ShowRacksWithoutHangerBarCommand { get; private set; }
        public RelayCommand CreateCustomerCommand { get; private set; }
        public RelayCommand CreateContractCommand { get; private set; }
        public RelayCommand ClearSelectionCommand { get; private set; }
        public RelayCommand ShowNeighborRacksCommand { get; private set; }
        public RelayCommand OpenBarcodeWindowCommand { get; private set; }
        public RelayCommand OpenScannerWindowCommand { get; private set; }
        public RelayCommand OpenInvoiceWindowCommand { get; private set; }
        public RelayCommand OpenTerminationWindowCommand { get; private set; }

        private void CreateCommands()
        {
            ShowAvailableRacksCommand = new RelayCommand(ShowAvailableRacks);
            ShowRacksWithoutHangerBarCommand = new RelayCommand(ShowRacksWithoutHangerBar);
            CreateCustomerCommand = new RelayCommand(CreateCustomer, _ => CanCreateCustomer);
            CreateContractCommand = new RelayCommand(CreateContract, _ => CanCreateContract);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            ShowNeighborRacksCommand = new RelayCommand(ShowNeighborRacks);

            OpenBarcodeWindowCommand = new RelayCommand(_ => _windowService.ShowBarcodeWindow());
            OpenScannerWindowCommand = new RelayCommand(_ => _windowService.ShowScannerWindow());
            OpenInvoiceWindowCommand = new RelayCommand(_ => _windowService.ShowInvoiceWindow());
            OpenTerminationWindowCommand = new RelayCommand(_ => _windowService.ShowTerminationWindow());
        }

        private void LoadCustomersFromDatabase()
        {
            try
            {
                // Hent kunder fra databasen
                var customersFromDb = _customerService.GetAllCustomers();

                // Clear og genindlæs
                Customers.Clear();
                foreach (var customer in customersFromDb)
                {
                    Customers.Add(new CustomerViewModel(customer));
                }

                StatusMessage = $"Loaded {Customers.Count} kunder fra databasen";

                // Debug output
                System.Diagnostics.Debug.WriteLine($"Database customers loaded: {Customers.Count}");
                foreach (var customer in Customers.Take(3))
                {
                    System.Diagnostics.Debug.WriteLine($"- {customer.Name} ({customer.Phone})");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fejl ved indlæsning fra database: {ex.Message}",
                               "Database Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Fejl ved database indlæsning";
            }
        }

        private void LoadRacksFromDatabase()
        {
            try
            {
                // Hent reoler fra databasen
                var racks = _rackRepository.GetAll().Where(r => r.IsAvailable);
                AvailableRacks = new ObservableCollection<RackViewModel>(
                    racks.Select(r => new RackViewModel(r)));

                System.Diagnostics.Debug.WriteLine($"Loaded {AvailableRacks.Count} available racks from database");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading racks: {ex.Message}");
            }
        }

        private void LoadCustomerRacks()
        {
            if (SelectedCustomer != null)
            {
                var racks = _rentalService.GetRacksForCustomer(SelectedCustomer.CustomerId);
                CustomerRacks = new ObservableCollection<RackViewModel>(
                    racks.Select(r => new RackViewModel(r)));
            }
        }

        private void LoadNeighborRacks()
        {
            if (SelectedCustomerRack != null)
            {
                var neighbors = _rackRepository.GetAll()
                    .Where(r => r.IsAvailable &&
                                Math.Abs(r.RackNumber - SelectedCustomerRack.RackNumber) == 1);

                NeighborRacks = new ObservableCollection<RackViewModel>(
                    neighbors.Select(r => new RackViewModel(r)));
            }
        }

        private void ShowAvailableRacks(object parameter)
        {
            var racks = _rackRepository.GetAll().Where(r => r.IsAvailable);
            AvailableRacks = new ObservableCollection<RackViewModel>(
                racks.Select(r => new RackViewModel(r)));
            StatusMessage = $"Viser {AvailableRacks.Count} ledige reoler";
        }

        private void ShowRacksWithoutHangerBar(object parameter)
        {
            var racks = _rackRepository.GetAll()
                .Where(r => r.IsAvailable && !r.HasHangerBar);
            AvailableRacks = new ObservableCollection<RackViewModel>(
                racks.Select(r => new RackViewModel(r)));
            StatusMessage = $"Viser {AvailableRacks.Count} reoler uden bøjlestang";
        }

        private void CreateCustomer(object parameter)
        {
            try
            {
                var newCustomer = _customerService.CreateCustomer(
                    NewCustomerName,
                    NewCustomerPhone,
                    NewCustomerEmail,
                    NewCustomerAddress);

                // Reload customers from database
                LoadCustomersFromDatabase();

                SelectedCustomer = Customers.FirstOrDefault(c => c.CustomerId == newCustomer.CustomerId);
                ClearCustomerForm();
                StatusMessage = $"Oprettet kunde: {newCustomer.Name}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Fejl ved oprettelse af kunde";
            }
        }

        private void CreateContract(object parameter)
        {
            if (SelectedCustomer == null)
                return;

            try
            {
                int rackId = 0;
                if (SelectedRack != null)
                {
                    rackId = SelectedRack.RackId;
                }
                else if (NeighborRacks != null && NeighborRacks.Any())
                {
                    rackId = NeighborRacks.First().RackId;
                }

                if (rackId == 0)
                    return;

                var agreement = _rentalService.CreateRentalAgreement(
                    SelectedCustomer.CustomerId,
                    rackId,
                    DateTime.Now);

                if (agreement != null)
                {
                    LoadRacksFromDatabase();
                    LoadCustomerRacks();

                    MessageBox.Show(
                        $"Lejeaftale oprettet!\nKunde: {SelectedCustomer.Name}\nReol: {agreement.RackId}\nMånedlig leje: {agreement.MonthlyRent:C0}",
                        "Lejeaftale oprettet",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ClearSelection(null);
                    StatusMessage = "Lejeaftale oprettet succesfuldt";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Fejl ved oprettelse af lejeaftale";
            }
        }

        private void ShowNeighborRacks(object parameter)
        {
            if (SelectedCustomer != null && SelectedCustomerRack != null)
            {
                LoadNeighborRacks();
                StatusMessage = $"Viser {NeighborRacks.Count} ledige nabo-reoler";
            }
        }

        private void ClearSelection(object parameter)
        {
            SelectedRack = null;
            SelectedCustomer = null;
            SelectedCustomerRack = null;
            ClearCustomerForm();
            CustomerRacks.Clear();
            NeighborRacks.Clear();
            StatusMessage = "Valg ryddet";
        }

        private void ClearCustomerForm()
        {
            NewCustomerName = string.Empty;
            NewCustomerPhone = string.Empty;
            NewCustomerEmail = string.Empty;
            NewCustomerAddress = string.Empty;
        }
    }
}