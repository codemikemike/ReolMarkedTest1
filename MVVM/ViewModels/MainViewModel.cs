using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Commands;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// Hoved ViewModel for ReolMarked applikationen
    /// Håndterer al logik mellem UI og data
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // Private felter til repositories
        private readonly RackRepository _rackRepository;
        private readonly CustomerRepository _customerRepository;

        // Private felter til UI binding
        private ObservableCollection<Rack> _availableRacks = new();
        private ObservableCollection<Customer> _customers = new();
        private Rack? _selectedRack;
        private Customer? _selectedCustomer;
        private string _newCustomerName = "";
        private string _newCustomerPhone = "";
        private string _newCustomerEmail = "";
        private string _newCustomerAddress = "";
        private string _statusMessage = "";

        // Konstruktør - opsætter repositories og kommandoer
        public MainViewModel()
        {
            // Opret repositories
            _rackRepository = new RackRepository();
            _customerRepository = new CustomerRepository();

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
        /// Den reol som brugeren har valgt
        /// </summary>
        public Rack? SelectedRack
        {
            get { return _selectedRack; }
            set
            {
                _selectedRack = value;
                OnPropertyChanged(nameof(SelectedRack));
                OnPropertyChanged(nameof(IsRackSelected));
                OnPropertyChanged(nameof(CanCreateContract)); // Tilføjet!

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
                OnPropertyChanged(nameof(CanCreateContract)); // Tilføjet!
            }
        }

        /// <summary>
        /// Navn for ny kunde (som Anton)
        /// </summary>
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

        /// <summary>
        /// Telefon for ny kunde
        /// </summary>
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

        /// <summary>
        /// Email for ny kunde
        /// </summary>
        public string NewCustomerEmail
        {
            get { return _newCustomerEmail; }
            set
            {
                _newCustomerEmail = value;
                OnPropertyChanged(nameof(NewCustomerEmail));
            }
        }

        /// <summary>
        /// Adresse for ny kunde
        /// </summary>
        public string NewCustomerAddress
        {
            get { return _newCustomerAddress; }
            set
            {
                _newCustomerAddress = value;
                OnPropertyChanged(nameof(NewCustomerAddress));
            }
        }

        /// <summary>
        /// Status besked til brugeren
        /// </summary>
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

        /// <summary>
        /// Om der er valgt en reol
        /// </summary>
        public bool IsRackSelected
        {
            get { return SelectedRack != null; }
        }

        /// <summary>
        /// Om der er valgt en kunde
        /// </summary>
        public bool IsCustomerSelected
        {
            get { return SelectedCustomer != null; }
        }

        /// <summary>
        /// Om der kan oprettes en ny kunde
        /// </summary>
        public bool CanCreateCustomer
        {
            get
            {
                return !string.IsNullOrEmpty(NewCustomerName) &&
                       !string.IsNullOrEmpty(NewCustomerPhone);
            }
        }

        /// <summary>
        /// Om der kan oprettes en kontrakt
        /// </summary>
        public bool CanCreateContract
        {
            get
            {
                return IsRackSelected && IsCustomerSelected;
            }
        }

        // Kommando properties til knapper

        public RelayCommand ShowAvailableRacksCommand { get; private set; }
        public RelayCommand ShowRacksWithoutHangerBarCommand { get; private set; }
        public RelayCommand CreateCustomerCommand { get; private set; }
        public RelayCommand CreateContractCommand { get; private set; }
        public RelayCommand ClearSelectionCommand { get; private set; }

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
        /// Opretter alle kommandoer til knapper
        /// </summary>
        private void CreateCommands()
        {
            ShowAvailableRacksCommand = new RelayCommand(ShowAvailableRacks);
            ShowRacksWithoutHangerBarCommand = new RelayCommand(ShowRacksWithoutHangerBar);
            CreateCustomerCommand = new RelayCommand(CreateCustomer, CanExecuteCreateCustomer);
            CreateContractCommand = new RelayCommand(CreateContract, CanExecuteCreateContract);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
        }

        // Kommando metoder (kaldt når knapper trykkes)

        /// <summary>
        /// Viser alle ledige reoler (som Mettes ledige kasse)
        /// </summary>
        private void ShowAvailableRacks(object? parameter)
        {
            AvailableRacks = _rackRepository.GetAvailableRacks();
            StatusMessage = $"Viser {AvailableRacks.Count} ledige reoler";
        }

        /// <summary>
        /// Viser kun reoler uden bøjlestang (som Anton ønskede)
        /// </summary>
        private void ShowRacksWithoutHangerBar(object? parameter)
        {
            AvailableRacks = _rackRepository.GetAvailableRacksWithoutHangerBar();
            StatusMessage = $"Viser {AvailableRacks.Count} ledige reoler uden bøjlestang";
        }

        /// <summary>
        /// Opretter ny kunde (når Anton beslutter sig)
        /// </summary>
        private void CreateCustomer(object? parameter)
        {
            // Opret kunden gennem repository
            var newCustomer = _customerRepository.AddCustomer(
                NewCustomerName,
                NewCustomerPhone,
                NewCustomerEmail,
                NewCustomerAddress);

            // Opdater kunde listen
            Customers = _customerRepository.GetActiveCustomers();

            // Vælg den nye kunde automatisk
            SelectedCustomer = newCustomer;

            // Ryd inputfelterne
            ClearCustomerForm();

            // Opdater status
            StatusMessage = $"Oprettet kunde: {newCustomer.CustomerName}";
        }

        /// <summary>
        /// Tjekker om der kan oprettes en kunde
        /// </summary>
        private bool CanExecuteCreateCustomer(object? parameter)
        {
            return CanCreateCustomer;
        }

        /// <summary>
        /// Opretter en reol kontrakt
        /// </summary>
        private void CreateContract(object? parameter)
        {
            if (SelectedRack != null && SelectedCustomer != null)
            {
                // Reserver reolen
                bool success = _rackRepository.ReserveRack(SelectedRack.RackNumber);

                if (success)
                {
                    // Opdater reol listen
                    AvailableRacks = _rackRepository.GetAvailableRacks();

                    // Vis bekræftelse
                    string message = $"Kontrakt oprettet!\n" +
                                   $"Kunde: {SelectedCustomer.CustomerName}\n" +
                                   $"Reol: {SelectedRack.RackNumber}\n" +
                                   $"Månedlig leje: 850 kr.";

                    MessageBox.Show(message, "Kontrakt oprettet", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Ryd valg
                    ClearSelection(null);

                    StatusMessage = "Kontrakt oprettet succesfuldt";
                }
                else
                {
                    StatusMessage = "Fejl: Kunne ikke reservere reol";
                }
            }
        }

        /// <summary>
        /// Tjekker om der kan oprettes en kontrakt
        /// </summary>
        private bool CanExecuteCreateContract(object? parameter)
        {
            return CanCreateContract;
        }

        /// <summary>
        /// Rydder alle valg
        /// </summary>
        private void ClearSelection(object? parameter)
        {
            SelectedRack = null;
            SelectedCustomer = null;
            ClearCustomerForm();
            StatusMessage = "Valg ryddet";
        }

        /// <summary>
        /// Rydder kunde formularen
        /// </summary>
        private void ClearCustomerForm()
        {
            NewCustomerName = "";
            NewCustomerPhone = "";
            NewCustomerEmail = "";
            NewCustomerAddress = "";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sender besked når en property ændres
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}