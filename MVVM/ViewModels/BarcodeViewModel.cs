using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for stregkode generering (UC3.2)
    /// Håndterer UI logik for kunder der vil oprette stregkoder
    /// </summary>
    public class BarcodeViewModel : INotifyPropertyChanged
    {
        // Private felter til services og repositories
        private readonly BarcodeService _barcodeService;
        private readonly CustomerRepository _customerRepository;
        private readonly RackRepository _rackRepository;
        private readonly RentalService _rentalService;

        // Private felter til UI binding
        private string _customerPhone = "";
        private Customer _selectedCustomer;
        private int _rackNumber;
        private ObservableCollection<Rack> _customerRacks = new();
        private ObservableCollection<LabelRequest> _labelRequests = new();
        private string _newProductName = "";
        private decimal _newProductPrice;
        private int _newProductQuantity = 1;
        private string _statusMessage = "";
        private string _printOutput = "";
        private BarcodeGenerationResult _lastResult;

        // Konstruktør - opsætter services
        public BarcodeViewModel()
        {
            // RETTET: Brug ServiceLocator i stedet for at oprette nye instanser
            _rackRepository = ServiceLocator.RackRepository;
            _customerRepository = ServiceLocator.CustomerRepository;
            _rentalService = ServiceLocator.RentalService;
            _barcodeService = ServiceLocator.BarcodeService;

            // Opret kommandoer
            CreateCommands();

            // Sæt initial status
            StatusMessage = "Indtast dit telefonnummer for at finde dine reoler";
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
                    StatusMessage = $"Velkommen {_selectedCustomer.CustomerName}! Vælg hvilken reol du vil lave stregkoder til.";
                }
                else
                {
                    CustomerRacks.Clear();
                }
            }
        }

        /// <summary>
        /// Valgt reolnummer
        /// </summary>
        public int RackNumber
        {
            get { return _rackNumber; }
            set
            {
                _rackNumber = value;
                OnPropertyChanged(nameof(RackNumber));
                OnPropertyChanged(nameof(IsRackSelected));
                OnPropertyChanged(nameof(CanAddProduct));

                if (_rackNumber > 0)
                {
                    StatusMessage = $"Reol {_rackNumber} valgt. Tilføj produkter du vil lave stregkoder til.";
                }
            }
        }

        /// <summary>
        /// Kundens reoler
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
        /// Liste over produkter kunden vil lave stregkoder til
        /// </summary>
        public ObservableCollection<LabelRequest> LabelRequests
        {
            get { return _labelRequests; }
            set
            {
                _labelRequests = value;
                OnPropertyChanged(nameof(LabelRequests));
                OnPropertyChanged(nameof(CanGenerateLabels));
                OnPropertyChanged(nameof(TotalLabels));
            }
        }

        /// <summary>
        /// Navn på nyt produkt
        /// </summary>
        public string NewProductName
        {
            get { return _newProductName; }
            set
            {
                _newProductName = value;
                OnPropertyChanged(nameof(NewProductName));
                OnPropertyChanged(nameof(CanAddProduct));
            }
        }

        /// <summary>
        /// Pris på nyt produkt
        /// </summary>
        public decimal NewProductPrice
        {
            get { return _newProductPrice; }
            set
            {
                _newProductPrice = value;
                OnPropertyChanged(nameof(NewProductPrice));
                OnPropertyChanged(nameof(CanAddProduct));
            }
        }

        /// <summary>
        /// Antal af nyt produkt
        /// </summary>
        public int NewProductQuantity
        {
            get { return _newProductQuantity; }
            set
            {
                _newProductQuantity = value;
                OnPropertyChanged(nameof(NewProductQuantity));
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

        /// <summary>
        /// Print output til stregkoder
        /// </summary>
        public string PrintOutput
        {
            get { return _printOutput; }
            set
            {
                _printOutput = value;
                OnPropertyChanged(nameof(PrintOutput));
                OnPropertyChanged(nameof(HasPrintOutput));
            }
        }

        /// <summary>
        /// Sidste generering resultat
        /// </summary>
        public BarcodeGenerationResult LastResult
        {
            get { return _lastResult; }
            set
            {
                _lastResult = value;
                OnPropertyChanged(nameof(LastResult));
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
        /// Om der er valgt en reol
        /// </summary>
        public bool IsRackSelected
        {
            get { return RackNumber > 0; }
        }

        /// <summary>
        /// Om der kan tilføjes et produkt
        /// </summary>
        public bool CanAddProduct
        {
            get
            {
                return IsRackSelected &&
                       !string.IsNullOrEmpty(NewProductName) &&
                       NewProductPrice > 0;
            }
        }

        /// <summary>
        /// Om der kan genereres stregkoder
        /// </summary>
        public bool CanGenerateLabels
        {
            get { return LabelRequests != null && LabelRequests.Count > 0; }
        }

        /// <summary>
        /// Om der er print output
        /// </summary>
        public bool HasPrintOutput
        {
            get { return !string.IsNullOrEmpty(PrintOutput); }
        }

        /// <summary>
        /// Totalt antal stregkoder der vil blive oprettet
        /// </summary>
        public int TotalLabels
        {
            get
            {
                int total = 0;
                if (LabelRequests != null)
                {
                    foreach (var request in LabelRequests)
                    {
                        total += request.Quantity;
                    }
                }
                return total;
            }
        }

        // Kommando properties
        public RelayCommand FindCustomerCommand { get; private set; }
        public RelayCommand AddProductCommand { get; private set; }
        public RelayCommand RemoveProductCommand { get; private set; }
        public RelayCommand GenerateLabelsCommand { get; private set; }
        public RelayCommand PrintCommand { get; private set; }
        public RelayCommand ClearAllCommand { get; private set; }

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
        /// Opretter alle kommandoer
        /// </summary>
        private void CreateCommands()
        {
            FindCustomerCommand = new RelayCommand(FindCustomer, CanExecuteFindCustomer);
            AddProductCommand = new RelayCommand(AddProduct, CanExecuteAddProduct);
            RemoveProductCommand = new RelayCommand(RemoveProduct);
            GenerateLabelsCommand = new RelayCommand(GenerateLabels, CanExecuteGenerateLabels);
            PrintCommand = new RelayCommand(PrintLabels, CanExecutePrint);
            ClearAllCommand = new RelayCommand(ClearAll);
        }

        // Kommando metoder

        /// <summary>
        /// Finder kunde baseret på telefonnummer
        /// </summary>
        private void FindCustomer(object parameter)
        {
            var customer = _barcodeService.FindCustomerByPhone(CustomerPhone);
            if (customer != null)
            {
                SelectedCustomer = customer;
            }
            else
            {
                MessageBox.Show("Kunde ikke fundet. Tjek dit telefonnummer.", "Kunde ikke fundet",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Kunde ikke fundet - prøv igen med dit telefonnummer";
            }
        }

        private bool CanExecuteFindCustomer(object parameter)
        {
            return CanFindCustomer;
        }

        /// <summary>
        /// Tilføjer et produkt til listen
        /// </summary>
        private void AddProduct(object parameter)
        {
            var newRequest = new LabelRequest
            {
                Name = NewProductName,
                Price = NewProductPrice,
                Quantity = NewProductQuantity
            };

            LabelRequests.Add(newRequest);

            // Vigtig: Send notification for TotalLabels så UI opdateres
            OnPropertyChanged(nameof(TotalLabels));
            OnPropertyChanged(nameof(CanGenerateLabels));

            // Ryd input felterne
            NewProductName = "";
            NewProductPrice = 0;
            NewProductQuantity = 1;

            StatusMessage = $"Produkt tilføjet. Total: {TotalLabels} stregkoder vil blive oprettet.";
        }

        private bool CanExecuteAddProduct(object parameter)
        {
            return CanAddProduct;
        }

        /// <summary>
        /// Fjerner et produkt fra listen
        /// </summary>
        private void RemoveProduct(object parameter)
        {
            if (parameter is LabelRequest request)
            {
                LabelRequests.Remove(request);

                // Vigtig: Send notification for TotalLabels så UI opdateres
                OnPropertyChanged(nameof(TotalLabels));
                OnPropertyChanged(nameof(CanGenerateLabels));

                StatusMessage = $"Produkt fjernet. Total: {TotalLabels} stregkoder vil blive oprettet.";
            }
        }

        /// <summary>
        /// Genererer alle stregkoder
        /// </summary>
        private void GenerateLabels(object parameter)
        {
            if (SelectedCustomer == null || RackNumber <= 0 || LabelRequests.Count == 0)
                return;

            // Konverter til List for BarcodeService
            var requestList = new System.Collections.Generic.List<LabelRequest>();
            foreach (var request in LabelRequests)
            {
                requestList.Add(request);
            }

            // Generer stregkoder via service
            var result = _barcodeService.GenerateLabelsForProducts(
                SelectedCustomer.CustomerId,
                RackNumber,
                requestList);

            LastResult = result;

            if (result.Success)
            {
                PrintOutput = result.PrintOutput;
                StatusMessage = $"Succesfuldt oprettet {result.LabelCount} stregkoder!";

                // Vis bekræftelse
                MessageBox.Show($"Stregkoder oprettet!\n\nAntal: {result.LabelCount}\nReol: {RackNumber}\n\nStregkoderne er klar til print.",
                    "Stregkoder oprettet", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteGenerateLabels(object parameter)
        {
            return CanGenerateLabels;
        }

        /// <summary>
        /// Printer stregkoder
        /// </summary>
        private void PrintLabels(object parameter)
        {
            if (!string.IsNullOrEmpty(PrintOutput))
            {
                try
                {
                    var printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        // Opret en DrawingVisual til print
                        var visual = new DrawingVisual();
                        using (var context = visual.RenderOpen())
                        {
                            var typeface = new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                            var fontSize = 10;
                            var brush = Brushes.Black;

                            var lines = PrintOutput.Split('\n');
                            double yPosition = 0;
                            double lineHeight = fontSize * 1.2;

                            foreach (var line in lines)
                            {
                                if (!string.IsNullOrEmpty(line.Trim()))
                                {
                                    var formattedText = new FormattedText(
                                        line,
                                        System.Globalization.CultureInfo.CurrentCulture,
                                        FlowDirection.LeftToRight,
                                        typeface,
                                        fontSize,
                                        brush,
                                        96); // DPI

                                    context.DrawText(formattedText, new Point(20, yPosition));
                                }
                                yPosition += lineHeight;
                            }
                        }

                        printDialog.PrintVisual(visual, "Stregkoder");
                        StatusMessage = "Stregkoder sendt til printer!";
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Fejl ved print: {ex.Message}", "Print fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecutePrint(object parameter)
        {
            return HasPrintOutput;
        }

        /// <summary>
        /// Rydder alt og starter forfra
        /// </summary>
        private void ClearAll(object parameter)
        {
            SelectedCustomer = null;
            CustomerPhone = "";
            RackNumber = 0;
            CustomerRacks.Clear();
            LabelRequests.Clear();
            NewProductName = "";
            NewProductPrice = 0;
            NewProductQuantity = 1;
            PrintOutput = "";
            LastResult = null;
            StatusMessage = "Indtast dit telefonnummer for at finde dine reoler";

            // Send notifications for alle relevante properties
            OnPropertyChanged(nameof(TotalLabels));
            OnPropertyChanged(nameof(CanGenerateLabels));
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}