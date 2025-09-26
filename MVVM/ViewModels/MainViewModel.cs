using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.Commands;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// Hoved ViewModel for ReolMarked applikationen
    /// Håndterer al logik mellem UI og data - nu med UC2 funktionalitet
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // Private felter til repositories og services
        private readonly RackRepository _rackRepository;
        private readonly CustomerRepository _customerRepository;
        private readonly RentalService _rentalService;

        // Private felter til UI binding
        private ObservableCollection<Rack> _availableRacks = new();
        private ObservableCollection<Customer> _customers = new();
        private ObservableCollection<Rack> _customerRacks = new(); // NYT - til UC2
        private ObservableCollection<Rack> _neighborRacks = new(); // NYT - til UC2

        private Rack? _selectedRack;
        private Customer? _selectedCustomer;
        private Rack? _selectedCustomerRack; // NYT - til UC2

        private string _newCustomerName = "";
        private string _newCustomerPhone = "";
        private string _newCustomerEmail = "";
        private string _newCustomerAddress = "";
        private string _statusMessage = "";

        // Konstruktør - opsætter repositories og services
        public MainViewModel()
        {
            // Opret repositories
            _rackRepository = new RackRepository();
            _customerRepository = new CustomerRepository();

            // Opret service med dependencies
            _rentalService = new RentalService(_customerRepository, _rackRepository);

            // Indlæs initial data
            LoadData();

            // Opret kommandoer til knapper
            CreateCommands();

            // Sæt initial status
            StatusMessage = "Klar til at hjælpe kunder";
        }

        // Properties til UI binding

        /// <summary>
        /// Liste over ledige reoler (som Mettes ledige kasse)
        /// </summary>
        public ObservableCollection<Rack> AvailableRacks
        {
            get { return _availableRacks; }
            set
            {
                _availableRacks = value;
                OnPropertyChanged(nameof(AvailableRacks));
            }
        }

        /// <summary>
        /// Liste over alle kunder
        /// </summary>
        public ObservableCollection<Customer> Customers
        {
            get { return _customers; }
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
            }
        }

        /// <summary>
        /// NYT - Liste over reoler som den valgte kunde allerede lejer
        /// </summary>
        public ObservableCollection<Rack> CustomerRacks
        {
            get { return _customerRacks; }
            set
            {
                _customerRacks = value;
                OnPropertyChanged(nameof(CustomerRacks));
            }
        }

        /// <summary>
        /// NYT - Liste over ledige nabo-reoler til kundens valgte reol
        /// </summary>
        public ObservableCollection<Rack> NeighborRacks
        {
            get { return _neighborRacks; }
            set
            {
                _neighborRacks = value;
                OnPropertyChanged(nameof(NeighborRacks));
            }
        }

        /// <summary>
        /// Den reol som brugeren har valgt fra ledige reoler
        /// </summary>
        public Rack? SelectedRack
        {
            get { return _selectedRack; }
            set
            {
                _selectedRack = value;
                OnPropertyChanged(nameof(SelectedRack));
                OnPropertyChanged(nameof(IsRackSelected));
                OnPropertyChanged(nameof(CanCreateContract));

                // Opdater status besked
                if (_selectedRack != null)
                {
                    StatusMessage = $"Valgt reol {_selectedRack.RackNumber} - {_selectedRack.Location}";
                }
            }
        }

        /// <summary>
        /// Den kunde som brugeren har valgt
        /// </summary>
        public Customer? SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                OnPropertyChanged(nameof(IsCustomerSelected));
                OnPropertyChanged(nameof(CanCreateContract));

                // NYT - Når kunde vælges, hent deres eksisterende reoler
                if (_selectedCustomer != null)
                {
                    LoadCustomerRacks();
                    StatusMessage = $"Valgt kunde: {_selectedCustomer.CustomerName}";
                }
                else
                {
                    CustomerRacks.Clear();
                    NeighborRacks.Clear();
                }
            }
        }

        /// <summary>
        /// NYT - Den reol kunden har valgt som reference for nabo-søgning
        /// </summary>
        public Rack? SelectedCustomerRack
        {
            get { return _selectedCustomerRack; }
            set
            {
                _selectedCustomerRack = value;
                OnPropertyChanged(nameof(SelectedCustomerRack));
                OnPropertyChanged(nameof(IsCustomerRackSelected));

                // Når kunde vælger en af sine reoler, find nabo-reoler
                if (_selectedCustomerRack != null && _selectedCustomer != null)
                {
                    LoadNeighborRacks();
                    StatusMessage = $"Viser nabo-reoler til reol {_selectedCustomerRack.RackNumber}";
                }
                else
                {
                    NeighborRacks.Clear();
                }
            }
        }

        // Eksisterende properties...
        public string NewCustomerName
        {
            get { return _newCustomerName; }
            set
            {
                _newCustomerName = value;
                OnPropertyChanged(nameof(NewCustomerName));
                OnPropertyChanged(nameof(CanCreateCustomer));
            }
        }

        public string NewCustomerPhone
        {
            get { return _newCustomerPhone; }
            set
            {
                _newCustomerPhone = value;
                OnPropertyChanged(nameof(NewCustomerPhone));
                OnPropertyChanged(nameof(CanCreateCustomer));
            }
        }

        public string NewCustomerEmail
        {
            get { return _newCustomerEmail; }
            set
            {
                _newCustomerEmail = value;
                OnPropertyChanged(nameof(NewCustomerEmail));
            }
        }

        public string NewCustomerAddress
        {
            get { return _newCustomerAddress; }
            set
            {
                _newCustomerAddress = value;
                OnPropertyChanged(nameof(NewCustomerAddress));
            }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        // Beregnet properties til UI kontrol
        public bool IsRackSelected
        {
            get { return SelectedRack != null; }
        }

        public bool IsCustomerSelected
        {
            get { return SelectedCustomer != null; }
        }

        /// <summary>
        /// NYT - Om der er valgt en kunde-reol for nabo-søgning
        /// </summary>
        public bool IsCustomerRackSelected
        {
            get { return SelectedCustomerRack != null; }
        }

        public bool CanCreateCustomer
        {
            get
            {
                return !string.IsNullOrEmpty(NewCustomerName) &&
                       !string.IsNullOrEmpty(NewCustomerPhone);
            }
        }

        /// <summary>
        /// Opdateret - kan oprette kontrakt enten fra ledige reoler eller nabo-reoler
        /// </summary>
        public bool CanCreateContract
        {
            get
            {
                return IsCustomerSelected && (IsRackSelected || (NeighborRacks != null && NeighborRacks.Count > 0));
            }
        }

        // Kommando properties til knapper
        public RelayCommand ShowAvailableRacksCommand { get; private set; }
        public RelayCommand ShowRacksWithoutHangerBarCommand { get; private set; }
        public RelayCommand CreateCustomerCommand { get; private set; }
        public RelayCommand CreateContractCommand { get; private set; }
        public RelayCommand ClearSelectionCommand { get; private set; }
        public RelayCommand ShowNeighborRacksCommand { get; private set; } // NYT - til UC2

        // Private metoder

        /// <summary>
        /// Indlæser data fra repositories
        /// </summary>
        private void LoadData()
        {
            AvailableRacks = _rackRepository.GetAvailableRacks();
            Customers = _customerRepository.GetActiveCustomers();
        }

        /// <summary>
        /// NYT - Indlæser den valgte kundes eksisterende reoler
        /// </summary>
        private void LoadCustomerRacks()
        {
            if (SelectedCustomer != null)
            {
                CustomerRacks = _rentalService.GetRacksForCustomer(SelectedCustomer.CustomerId);
            }
        }

        /// <summary>
        /// NYT - Indlæser nabo-reoler til kundens valgte reol
        /// </summary>
        private void LoadNeighborRacks()
        {
            if (SelectedCustomerRack != null)
            {
                NeighborRacks = _rackRepository.GetAvailableNeighborRacks(SelectedCustomerRack.RackNumber);
            }
        }

        /// <summary>
        /// Opretter alle kommandoer til knapper
        /// </summary>
        private void CreateCommands()
        {
            ShowAvailableRacksCommand = new RelayCommand(ShowAvailableRacks);
            ShowRacksWithoutHangerBarCommand = new RelayCommand(ShowRacksWithoutHangerBar);
            CreateCustomerCommand = new RelayCommand(CreateCustomer, CanExecuteCreateCustomer);
            CreateContractCommand = new RelayCommand(CreateContract, CanExecuteCreateContract);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            ShowNeighborRacksCommand = new RelayCommand(ShowNeighborRacks); // NYT
        }

        // Kommando metoder

        private void ShowAvailableRacks(object? parameter)
        {
            AvailableRacks = _rackRepository.GetAvailableRacks();
            StatusMessage = $"Viser {AvailableRacks.Count} ledige reoler";
        }

        private void ShowRacksWithoutHangerBar(object? parameter)
        {
            AvailableRacks = _rackRepository.GetAvailableRacksWithoutHangerBar();
            StatusMessage = $"Viser {AvailableRacks.Count} ledige reoler uden bøjlestang";
        }

        /// <summary>
        /// NYT - Viser nabo-reoler for alle kundens reoler
        /// </summary>
        private void ShowNeighborRacks(object? parameter)
        {
            if (SelectedCustomer != null)
            {
                NeighborRacks = _rentalService.GetAvailableNeighborRacksForCustomer(SelectedCustomer.CustomerId);
                StatusMessage = $"Viser {NeighborRacks.Count} ledige nabo-reoler";
            }
        }

        private void CreateCustomer(object? parameter)
        {
            var newCustomer = _customerRepository.AddCustomer(
                NewCustomerName,
                NewCustomerPhone,
                NewCustomerEmail,
                NewCustomerAddress);

            Customers = _customerRepository.GetActiveCustomers();
            SelectedCustomer = newCustomer;
            ClearCustomerForm();
            StatusMessage = $"Oprettet kunde: {newCustomer.CustomerName}";
        }

        private bool CanExecuteCreateCustomer(object? parameter)
        {
            return CanCreateCustomer;
        }

        /// <summary>
        /// Opdateret - bruger nu RentalService til at oprette kontrakt
        /// </summary>
        private void CreateContract(object? parameter)
        {
            if (SelectedCustomer == null)
                return;

            Rack rackToRent = null;

            // Determine which rack to rent
            if (SelectedRack != null)
            {
                rackToRent = SelectedRack;
            }
            else if (NeighborRacks != null && NeighborRacks.Count > 0)
            {
                // For now, take the first neighbor rack
                // In a real UI, user would select from NeighborRacks
                rackToRent = NeighborRacks[0];
            }

            if (rackToRent != null)
            {
                var agreement = _rentalService.CreateRentalAgreement(
                    SelectedCustomer,
                    rackToRent,
                    System.DateTime.Now);

                if (agreement != null)
                {
                    // Opdater data
                    AvailableRacks = _rackRepository.GetAvailableRacks();
                    LoadCustomerRacks();
                    LoadNeighborRacks();

                    // Vis bekræftelse
                    string message = $"Lejeaftale oprettet!\n" +
                                   $"Kunde: {SelectedCustomer.CustomerName}\n" +
                                   $"Reol: {rackToRent.RackNumber}\n" +
                                   $"Månedlig leje: {agreement.PriceFormatted}";

                    MessageBox.Show(message, "Lejeaftale oprettet", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearSelection(null);
                    StatusMessage = "Lejeaftale oprettet succesfuldt";
                }
                else
                {
                    StatusMessage = "Fejl: Kunne ikke oprette lejeaftale";
                }
            }
        }

        private bool CanExecuteCreateContract(object? parameter)
        {
            return CanCreateContract;
        }

        private void ClearSelection(object? parameter)
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
            NewCustomerName = "";
            NewCustomerPhone = "";
            NewCustomerEmail = "";
            NewCustomerAddress = "";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}