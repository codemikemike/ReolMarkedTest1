using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.Services.DTOs;
using ReolMarked.MVVM.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for stregkode generering
    /// Kommunikerer KUN med Services
    /// </summary>
    public class BarcodeViewModel : ViewModelBase
    {
        private readonly BarcodeService _barcodeService;
        private readonly CustomerService _customerService;
        private readonly RentalService _rentalService;

        private string _customerPhone = string.Empty;
        private CustomerViewModel _selectedCustomer;
        private int _rackNumber;
        private ObservableCollection<RackViewModel> _customerRacks;
        private ObservableCollection<LabelRequest> _labelRequests;
        private string _newProductName = string.Empty;
        private decimal _newProductPrice;
        private int _newProductQuantity = 1;
        private string _statusMessage = string.Empty;
        private string _printOutput = string.Empty;

        // RETTET: Parameterløs konstruktør med ServiceLocator
        public BarcodeViewModel()
        {
            // Hent services fra ServiceLocator
            _barcodeService = ServiceLocator.BarcodeService;
            _customerService = ServiceLocator.CustomerService;
            _rentalService = ServiceLocator.RentalService;

            _customerRacks = new ObservableCollection<RackViewModel>();
            _labelRequests = new ObservableCollection<LabelRequest>();

            CreateCommands();
            StatusMessage = "Indtast dit telefonnummer for at finde dine reoler";
        }

        // Properties
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
                        StatusMessage = $"Velkommen {value.Name}! Vælg hvilken reol du vil lave stregkoder til.";
                    }
                    else
                    {
                        CustomerRacks.Clear();
                    }
                }
            }
        }

        public int RackNumber
        {
            get => _rackNumber;
            set
            {
                if (SetProperty(ref _rackNumber, value))
                {
                    OnPropertyChanged(nameof(IsRackSelected));
                    OnPropertyChanged(nameof(CanAddProduct));
                    if (value > 0)
                        StatusMessage = $"Reol {value} valgt. Tilføj produkter.";
                }
            }
        }

        public ObservableCollection<RackViewModel> CustomerRacks
        {
            get => _customerRacks;
            set => SetProperty(ref _customerRacks, value);
        }

        public ObservableCollection<LabelRequest> LabelRequests
        {
            get => _labelRequests;
            set
            {
                if (SetProperty(ref _labelRequests, value))
                {
                    OnPropertyChanged(nameof(CanGenerateLabels));
                    OnPropertyChanged(nameof(TotalLabels));
                }
            }
        }

        public string NewProductName
        {
            get => _newProductName;
            set
            {
                if (SetProperty(ref _newProductName, value))
                    OnPropertyChanged(nameof(CanAddProduct));
            }
        }

        public decimal NewProductPrice
        {
            get => _newProductPrice;
            set
            {
                if (SetProperty(ref _newProductPrice, value))
                    OnPropertyChanged(nameof(CanAddProduct));
            }
        }

        public int NewProductQuantity
        {
            get => _newProductQuantity;
            set => SetProperty(ref _newProductQuantity, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string PrintOutput
        {
            get => _printOutput;
            set
            {
                if (SetProperty(ref _printOutput, value))
                    OnPropertyChanged(nameof(HasPrintOutput));
            }
        }

        // Computed properties
        public bool CanFindCustomer => !string.IsNullOrEmpty(CustomerPhone);
        public bool IsCustomerSelected => SelectedCustomer != null;
        public bool IsRackSelected => RackNumber > 0;
        public bool CanAddProduct => IsRackSelected && !string.IsNullOrEmpty(NewProductName) && NewProductPrice > 0;
        public bool CanGenerateLabels => LabelRequests != null && LabelRequests.Any();
        public bool HasPrintOutput => !string.IsNullOrEmpty(PrintOutput);
        public int TotalLabels => LabelRequests?.Sum(r => r.Quantity) ?? 0;

        // Commands
        public RelayCommand FindCustomerCommand { get; private set; }
        public RelayCommand AddProductCommand { get; private set; }
        public RelayCommand RemoveProductCommand { get; private set; }
        public RelayCommand GenerateLabelsCommand { get; private set; }
        public RelayCommand ClearAllCommand { get; private set; }

        private void LoadCustomerRacks()
        {
            if (SelectedCustomer != null)
            {
                var racks = _rentalService.GetRacksForCustomer(SelectedCustomer.CustomerId);
                CustomerRacks = new ObservableCollection<RackViewModel>(
                    racks.Select(r => new RackViewModel(r)));
            }
        }

        private void CreateCommands()
        {
            FindCustomerCommand = new RelayCommand(FindCustomer, _ => CanFindCustomer);
            AddProductCommand = new RelayCommand(AddProduct, _ => CanAddProduct);
            RemoveProductCommand = new RelayCommand(RemoveProduct);
            GenerateLabelsCommand = new RelayCommand(GenerateLabels, _ => CanGenerateLabels);
            ClearAllCommand = new RelayCommand(ClearAll);
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

        private void AddProduct(object parameter)
        {
            var request = new LabelRequest
            {
                Name = NewProductName,
                Price = NewProductPrice,
                Quantity = NewProductQuantity
            };

            LabelRequests.Add(request);
            OnPropertyChanged(nameof(TotalLabels));
            OnPropertyChanged(nameof(CanGenerateLabels));

            NewProductName = string.Empty;
            NewProductPrice = 0;
            NewProductQuantity = 1;

            StatusMessage = $"Produkt tilføjet. Total: {TotalLabels} stregkoder";
        }

        private void RemoveProduct(object parameter)
        {
            if (parameter is LabelRequest request)
            {
                LabelRequests.Remove(request);
                OnPropertyChanged(nameof(TotalLabels));
                OnPropertyChanged(nameof(CanGenerateLabels));
                StatusMessage = $"Produkt fjernet. Total: {TotalLabels} stregkoder";
            }
        }

        private void GenerateLabels(object parameter)
        {
            if (SelectedCustomer == null || RackNumber <= 0)
                return;

            var result = _barcodeService.GenerateLabelsForProducts(
                SelectedCustomer.CustomerId,
                RackNumber,
                LabelRequests.ToList());

            if (result.Success)
            {
                PrintOutput = result.PrintOutput;
                StatusMessage = $"Succesfuldt oprettet {result.LabelCount} stregkoder!";
                MessageBox.Show($"Stregkoder oprettet!\n\nAntal: {result.LabelCount}\nReol: {RackNumber}",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearAll(object parameter)
        {
            SelectedCustomer = null;
            CustomerPhone = string.Empty;
            RackNumber = 0;
            CustomerRacks.Clear();
            LabelRequests.Clear();
            NewProductName = string.Empty;
            NewProductPrice = 0;
            NewProductQuantity = 1;
            PrintOutput = string.Empty;
            StatusMessage = "Indtast dit telefonnummer";
            OnPropertyChanged(nameof(TotalLabels));
            OnPropertyChanged(nameof(CanGenerateLabels));
        }
    }
}