using System;
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
    /// ViewModel for opsigelse af reoler (UC5.1)
    /// Håndterer UI logik for medarbejdere der skal registrere opsigelser
    /// </summary>
    public class TerminationViewModel : INotifyPropertyChanged
    {
        // Private felter til services og repositories
        private readonly TerminationService _terminationService;
        private readonly CustomerRepository _customerRepository;
        private readonly RackRepository _rackRepository;
        private readonly RentalService _rentalService;

        // Private felter til UI binding
        private string _customerPhone = "";
        private Customer _selectedCustomer;
        private ObservableCollection<Rack> _customerRacks = new();
        private int _selectedRackNumber;
        private DateTime _desiredTerminationDate;
        private string _terminationReason = "";
        private string _statusMessage = "";
        private bool _useCustomDate = false;

        // Lister til visning
        private ObservableCollection<RackTermination> _activeTerminations = new();
        private ObservableCollection<RackTermination> _customerTerminations = new();
        private ObservableCollection<RackTermination> _terminationsToProcess = new();
        private ObservableCollection<RackTermination> _processedTerminations = new(); // NYE - historik
        private RackTermination _selectedTermination;

        // Konstruktør - opsætter services
        public TerminationViewModel()
        {
            // Opret repositories og services
            _rackRepository = new RackRepository();
            _customerRepository = new CustomerRepository();
            _rentalService = new RentalService(_customerRepository, _rackRepository);
            _terminationService = new TerminationService(_customerRepository, _rackRepository, _rentalService);

            // Sæt standard værdier
            DesiredTerminationDate = DateTime.Now.Date.AddMonths(1);
            StatusMessage = "Find kunde for at registrere opsigelse";

            // Opret kommandoer
            CreateCommands();

            // Indlæs data
            LoadTerminationData();
        }

        // Properties til UI binding

        /// <summary>
        /// Kundens telefonnummer til at finde kunden
        /// </summary>
        public string CustomerPhone
        {
            get { return _customerPhone; }
            set
            {
                _customerPhone = value;
                OnPropertyChanged(nameof(CustomerPhone));
                OnPropertyChanged(nameof(CanFindCustomer));
            }
        }

        /// <summary>
        /// Den fundne kunde
        /// </summary>
        public Customer SelectedCustomer
        {
            get { return _selectedCustomer; }
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                OnPropertyChanged(nameof(IsCustomerSelected));

                if (_selectedCustomer != null)
                {
                    LoadCustomerRacks();
                    LoadCustomerTerminations();
                    StatusMessage = $"Kunde fundet: {_selectedCustomer.CustomerName}. Vælg reol til opsigelse.";
                }
                else
                {
                    CustomerRacks.Clear();
                    CustomerTerminations.Clear();
                }
            }
        }

        /// <summary>
        /// Kundens aktive reoler
        /// </summary>
        public ObservableCollection<Rack> CustomerRacks
        {
            get { return _customerRacks; }
            set
            {
                _customerRacks = value;
                OnPropertyChanged(nameof(CustomerRacks));
                OnPropertyChanged(nameof(HasCustomerRacks));
            }
        }

        /// <summary>
        /// Valgt reolnummer til opsigelse
        /// </summary>
        public int SelectedRackNumber
        {
            get { return _selectedRackNumber; }
            set
            {
                _selectedRackNumber = value;
                OnPropertyChanged(nameof(SelectedRackNumber));
                OnPropertyChanged(nameof(IsRackSelected));
                OnPropertyChanged(nameof(CanCreateTermination));

                if (_selectedRackNumber > 0 && !UseCustomDate)
                {
                    CalculateAutomaticDate();
                }
            }
        }

        /// <summary>
        /// Ønsket opsigelsesdato
        /// </summary>
        public DateTime DesiredTerminationDate
        {
            get { return _desiredTerminationDate; }
            set
            {
                _desiredTerminationDate = value;
                OnPropertyChanged(nameof(DesiredTerminationDate));
                OnPropertyChanged(nameof(DesiredTerminationDateFormatted));
            }
        }

        /// <summary>
        /// Om der bruges en specifik dato i stedet for automatisk beregning
        /// </summary>
        public bool UseCustomDate
        {
            get { return _useCustomDate; }
            set
            {
                _useCustomDate = value;
                OnPropertyChanged(nameof(UseCustomDate));
                OnPropertyChanged(nameof(UseAutomaticDate));
                // Fjern automatisk beregning helt - lad brugeren kontrollere datoen
            }
        }

        /// <summary>
        /// Årsag til opsigelsen
        /// </summary>
        public string TerminationReason
        {
            get { return _terminationReason; }
            set
            {
                _terminationReason = value;
                OnPropertyChanged(nameof(TerminationReason));
            }
        }

        /// <summary>
        /// Status besked til medarbejderen
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

        /// <summary>
        /// Alle aktive opsigelser i systemet
        /// </summary>
        public ObservableCollection<RackTermination> ActiveTerminations
        {
            get { return _activeTerminations; }
            set
            {
                _activeTerminations = value;
                OnPropertyChanged(nameof(ActiveTerminations));
                OnPropertyChanged(nameof(HasActiveTerminations));
            }
        }

        /// <summary>
        /// Kundens eksisterende opsigelser
        /// </summary>
        public ObservableCollection<RackTermination> CustomerTerminations
        {
            get { return _customerTerminations; }
            set
            {
                _customerTerminations = value;
                OnPropertyChanged(nameof(CustomerTerminations));
                OnPropertyChanged(nameof(HasCustomerTerminations));
            }
        }

        /// <summary>
        /// Opsigelser der skal behandles (trådt i kraft)
        /// </summary>
        public ObservableCollection<RackTermination> TerminationsToProcess
        {
            get { return _terminationsToProcess; }
            set
            {
                _terminationsToProcess = value;
                OnPropertyChanged(nameof(TerminationsToProcess));
                OnPropertyChanged(nameof(HasTerminationsToProcess));
            }
        }

        /// <summary>
        /// Behandlede opsigelser (historik)
        /// </summary>
        public ObservableCollection<RackTermination> ProcessedTerminations
        {
            get { return _processedTerminations; }
            set
            {
                _processedTerminations = value;
                OnPropertyChanged(nameof(ProcessedTerminations));
                OnPropertyChanged(nameof(HasProcessedTerminations));
            }
        }

        /// <summary>
        /// Valgt opsigelse i oversigten
        /// </summary>
        public RackTermination SelectedTermination
        {
            get { return _selectedTermination; }
            set
            {
                _selectedTermination = value;
                OnPropertyChanged(nameof(SelectedTermination));
                OnPropertyChanged(nameof(IsTerminationSelected));
            }
        }

        // Beregnet properties til UI kontrol

        /// <summary>
        /// Om der kan søges efter kunde
        /// </summary>
        public bool CanFindCustomer
        {
            get { return !string.IsNullOrEmpty(CustomerPhone); }
        }

        /// <summary>
        /// Om der er valgt en kunde
        /// </summary>
        public bool IsCustomerSelected
        {
            get { return SelectedCustomer != null; }
        }

        /// <summary>
        /// Om kunden har reoler
        /// </summary>
        public bool HasCustomerRacks
        {
            get { return CustomerRacks != null && CustomerRacks.Count > 0; }
        }

        /// <summary>
        /// Om der er valgt en reol
        /// </summary>
        public bool IsRackSelected
        {
            get { return SelectedRackNumber > 0; }
        }

        /// <summary>
        /// Om der kan oprettes en opsigelse
        /// </summary>
        public bool CanCreateTermination
        {
            get
            {
                return IsCustomerSelected && IsRackSelected &&
                       _terminationService.CanCustomerTerminateRack(SelectedCustomer.CustomerId, SelectedRackNumber);
            }
        }

        /// <summary>
        /// Om der bruges automatisk datoberegning
        /// </summary>
        public bool UseAutomaticDate
        {
            get { return !UseCustomDate; }
        }

        /// <summary>
        /// Om der er aktive opsigelser
        /// </summary>
        public bool HasActiveTerminations
        {
            get { return ActiveTerminations != null && ActiveTerminations.Count > 0; }
        }

        /// <summary>
        /// Om kunden har opsigelser
        /// </summary>
        public bool HasCustomerTerminations
        {
            get { return CustomerTerminations != null && CustomerTerminations.Count > 0; }
        }

        /// <summary>
        /// Om der er opsigelser at behandle
        /// </summary>
        public bool HasTerminationsToProcess
        {
            get { return TerminationsToProcess != null && TerminationsToProcess.Count > 0; }
        }

        /// <summary>
        /// Om der er behandlede opsigelser
        /// </summary>
        public bool HasProcessedTerminations
        {
            get { return ProcessedTerminations != null && ProcessedTerminations.Count > 0; }
        }

        /// <summary>
        /// Om der er valgt en opsigelse
        /// </summary>
        public bool IsTerminationSelected
        {
            get { return SelectedTermination != null; }
        }

        // Formaterede værdier
        public string DesiredTerminationDateFormatted
        {
            get { return DesiredTerminationDate.ToString("dd/MM/yyyy"); }
        }

        // Kommando properties
        public RelayCommand FindCustomerCommand { get; private set; }
        public RelayCommand CreateTerminationCommand { get; private set; }
        public RelayCommand CancelTerminationCommand { get; private set; }
        public RelayCommand ProcessEffectiveTerminationsCommand { get; private set; }
        public RelayCommand RefreshDataCommand { get; private set; }
        public RelayCommand ClearSelectionCommand { get; private set; }

        // Private metoder

        /// <summary>
        /// Indlæser kundens reoler
        /// </summary>
        private void LoadCustomerRacks()
        {
            if (SelectedCustomer != null)
            {
                CustomerRacks = _rentalService.GetRacksForCustomer(SelectedCustomer.CustomerId);
            }
        }

        /// <summary>
        /// Indlæser kundens opsigelser
        /// </summary>
        private void LoadCustomerTerminations()
        {
            if (SelectedCustomer != null)
            {
                CustomerTerminations = _terminationService.GetTerminationsForCustomer(SelectedCustomer.CustomerId);
            }
        }

        /// <summary>
        /// Indlæser alle opsigelsesdata
        /// </summary>
        private void LoadTerminationData()
        {
            ActiveTerminations = _terminationService.GetActiveTerminations();
            TerminationsToProcess = _terminationService.GetTerminationsToProcess();
            ProcessedTerminations = _terminationService.GetProcessedTerminations(); // NYE - historik
        }

        /// <summary>
        /// Beregner automatisk opsigelsesdato baseret på dagens dato
        /// </summary>
        private void CalculateAutomaticDate()
        {
            if (!UseCustomDate)
            {
                var tempTermination = new RackTermination
                {
                    RequestDate = DateTime.Now.Date
                };
                tempTermination.CalculateEffectiveDate();
                DesiredTerminationDate = tempTermination.EffectiveDate;
            }
        }

        /// <summary>
        /// Opretter alle kommandoer
        /// </summary>
        private void CreateCommands()
        {
            FindCustomerCommand = new RelayCommand(FindCustomer, CanExecuteFindCustomer);
            CreateTerminationCommand = new RelayCommand(CreateTermination, CanExecuteCreateTermination);
            CancelTerminationCommand = new RelayCommand(CancelTermination);
            ProcessEffectiveTerminationsCommand = new RelayCommand(ProcessEffectiveTerminations);
            RefreshDataCommand = new RelayCommand(RefreshData);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
        }

        // Kommando metoder

        /// <summary>
        /// Finder kunde baseret på telefonnummer
        /// </summary>
        private void FindCustomer(object parameter)
        {
            var customer = _customerRepository.GetCustomerByPhone(CustomerPhone);
            if (customer != null)
            {
                SelectedCustomer = customer;
            }
            else
            {
                MessageBox.Show("Kunde ikke fundet. Tjek telefonnummeret.", "Kunde ikke fundet",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Kunde ikke fundet - prøv igen";
            }
        }

        private bool CanExecuteFindCustomer(object parameter)
        {
            return CanFindCustomer;
        }

        /// <summary>
        /// Opretter en opsigelse (UC5.1 hovedfunktion)
        /// </summary>
        private void CreateTermination(object parameter)
        {
            if (SelectedCustomer == null || SelectedRackNumber <= 0)
                return;

            DateTime? desiredDate = null;

            // Kun send ønsket dato hvis brugeren specifikt har valgt at bruge en brugerdefineret dato
            if (UseCustomDate)
            {
                desiredDate = DesiredTerminationDate;
            }

            var result = _terminationService.CreateTermination(
                SelectedCustomer.CustomerId,
                SelectedRackNumber,
                desiredDate,
                TerminationReason);

            if (result.Success)
            {
                string message = $"Opsigelse oprettet!\n\n" +
                               $"Kunde: {SelectedCustomer.CustomerName}\n" +
                               $"Reol: {SelectedRackNumber}\n" +
                               $"Træder i kraft: {result.Termination.EffectiveDateFormatted}\n" +
                               $"Regel: {result.Termination.TerminationRuleText}";

                if (!string.IsNullOrEmpty(TerminationReason))
                {
                    message += $"\nÅrsag: {TerminationReason}";
                }

                MessageBox.Show(message, "Opsigelse oprettet", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = result.Message;
                LoadTerminationData();
                LoadCustomerTerminations();

                // Ryd formular
                SelectedRackNumber = 0;
                TerminationReason = "";
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteCreateTermination(object parameter)
        {
            return CanCreateTermination;
        }

        /// <summary>
        /// Annullerer en opsigelse
        /// </summary>
        private void CancelTermination(object parameter)
        {
            if (parameter is RackTermination termination)
            {
                var dialogResult = MessageBox.Show(
                    $"Er du sikker på at du vil annullere opsigelsen for {termination.CustomerName} - {termination.RackNumberDisplay}?",
                    "Annuller opsigelse", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (dialogResult == MessageBoxResult.Yes)
                {
                    var result = _terminationService.CancelTermination(termination.TerminationId, "Annulleret af medarbejder");

                    if (result.Success)
                    {
                        MessageBox.Show(result.Message, "Opsigelse annulleret", MessageBoxButton.OK, MessageBoxImage.Information);
                        StatusMessage = result.Message;
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

        /// <summary>
        /// Behandler opsigelser der er trådt i kraft
        /// </summary>
        private void ProcessEffectiveTerminations(object parameter)
        {
            var terminationsToProcess = _terminationService.GetTerminationsToProcess();

            if (terminationsToProcess.Count == 0)
            {
                MessageBox.Show("Ingen opsigelser at behandle i øjeblikket.", "Ingen opsigelser",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialogResult = MessageBox.Show(
                $"Der er {terminationsToProcess.Count} opsigelser der skal behandles.\n\nVil du gennemføre dem nu?",
                "Behandl opsigelser", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                var result = _terminationService.ProcessEffectiveTerminations();

                if (result.Success)
                {
                    string message = $"Behandling gennemført!\n\n" +
                                   $"Antal behandlede: {result.ProcessedTerminations.Count}\n\n" +
                                   "Reoler er frigivet og lejeaftaler afsluttet.";

                    MessageBox.Show(message, "Opsigelser behandlet", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusMessage = result.Message;
                    LoadTerminationData();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Genindlæser alle data
        /// </summary>
        private void RefreshData(object parameter)
        {
            LoadTerminationData();
            if (SelectedCustomer != null)
            {
                LoadCustomerTerminations();
            }
            StatusMessage = "Data genindlæst";
        }

        /// <summary>
        /// Rydder valgte kunde og formular
        /// </summary>
        private void ClearSelection(object parameter)
        {
            SelectedCustomer = null;
            CustomerPhone = "";
            SelectedRackNumber = 0;
            TerminationReason = "";
            UseCustomDate = false;
            StatusMessage = "Find kunde for at registrere opsigelse";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}