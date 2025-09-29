using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel for scanner funktionalitet (UC3.1)
    /// Håndterer Jonas' scanning af Ayas produkter
    /// </summary>
    public class ScannerViewModel : INotifyPropertyChanged
    {
        // Private felter til services
        private readonly SaleService _saleService;
        private readonly BarcodeService _barcodeService;
        private readonly CustomerRepository _customerRepository;
        private readonly RackRepository _rackRepository;
        private readonly RentalService _rentalService;

        // Private felter til UI binding
        private Sale _currentSale;
        private ObservableCollection<SaleLine> _scannedProducts = new();
        private string _barcodeInput = "";
        private decimal _totalAmount;
        private decimal _paidAmount;
        private decimal _changeAmount;
        private string _paymentMethod = "Kontant";
        private string _statusMessage = "";
        private bool _isPaymentMode = false;

        // Konstruktør - opsætter services og starter nyt salg
        public ScannerViewModel()
        {
            // Opret repositories og services
            _rackRepository = new RackRepository();
            _customerRepository = new CustomerRepository();
            _rentalService = new RentalService(_customerRepository, _rackRepository);
            _barcodeService = new BarcodeService(_customerRepository, _rackRepository, _rentalService);
            _saleService = new SaleService(_barcodeService);

            // Start nyt salg
            StartNewSale();

            // Opret kommandoer
            CreateCommands();

            // Sæt initial status
            StatusMessage = "Scanner klar - scan første produkt";
        }

        // Properties til UI binding

        /// <summary>
        /// Nuværende salg
        /// </summary>
        public Sale CurrentSale
        {
            get { return _currentSale; }
            set
            {
                _currentSale = value;
                OnPropertyChanged(nameof(CurrentSale));
                OnPropertyChanged(nameof(HasProducts));
                OnPropertyChanged(nameof(CanProceedToPayment));
            }
        }

        /// <summary>
        /// Liste over scannede produkter
        /// </summary>
        public ObservableCollection<SaleLine> ScannedProducts
        {
            get { return _scannedProducts; }
            set
            {
                _scannedProducts = value;
                OnPropertyChanged(nameof(ScannedProducts));
                OnPropertyChanged(nameof(HasProducts));
                OnPropertyChanged(nameof(ProductCount));
            }
        }

        /// <summary>
        /// Input felt til stregkode (scanner input)
        /// </summary>
        public string BarcodeInput
        {
            get { return _barcodeInput; }
            set
            {
                _barcodeInput = value;
                OnPropertyChanged(nameof(BarcodeInput));
                OnPropertyChanged(nameof(CanScanBarcode));
            }
        }

        /// <summary>
        /// Samlet beløb for salget
        /// </summary>
        public decimal TotalAmount
        {
            get { return _totalAmount; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged(nameof(TotalAmount));
                OnPropertyChanged(nameof(TotalAmountFormatted));
                CalculateChange();
            }
        }

        /// <summary>
        /// Beløb kunden har betalt
        /// </summary>
        public decimal PaidAmount
        {
            get { return _paidAmount; }
            set
            {
                _paidAmount = value;
                OnPropertyChanged(nameof(PaidAmount));
                OnPropertyChanged(nameof(PaidAmountFormatted));
                OnPropertyChanged(nameof(CanCompletePayment));
                CalculateChange();
            }
        }

        /// <summary>
        /// Byttepenge til kunden
        /// </summary>
        public decimal ChangeAmount
        {
            get { return _changeAmount; }
            set
            {
                _changeAmount = value;
                OnPropertyChanged(nameof(ChangeAmount));
                OnPropertyChanged(nameof(ChangeAmountFormatted));
            }
        }

        /// <summary>
        /// Betalingsmetode (Kontant, MobilePay)
        /// </summary>
        public string PaymentMethod
        {
            get { return _paymentMethod; }
            set
            {
                _paymentMethod = value;
                OnPropertyChanged(nameof(PaymentMethod));
            }
        }

        /// <summary>
        /// Status besked til Jonas
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
        /// Om vi er i betalings mode
        /// </summary>
        public bool IsPaymentMode
        {
            get { return _isPaymentMode; }
            set
            {
                _isPaymentMode = value;
                OnPropertyChanged(nameof(IsPaymentMode));
                OnPropertyChanged(nameof(IsScanningMode));
            }
        }

        // Beregnet properties til UI kontrol

        /// <summary>
        /// Om der er produkter i salget
        /// </summary>
        public bool HasProducts
        {
            get { return ScannedProducts.Count > 0; }
        }

        /// <summary>
        /// Om vi er i scanning mode
        /// </summary>
        public bool IsScanningMode
        {
            get { return !IsPaymentMode; }
        }

        /// <summary>
        /// Antal produkter i salget
        /// </summary>
        public int ProductCount
        {
            get { return ScannedProducts.Count; }
        }

        /// <summary>
        /// Om der kan scannes en stregkode
        /// </summary>
        public bool CanScanBarcode
        {
            get { return !string.IsNullOrEmpty(BarcodeInput) && IsScanningMode; }
        }

        /// <summary>
        /// Om vi kan gå til betaling
        /// </summary>
        public bool CanProceedToPayment
        {
            get { return HasProducts && IsScanningMode; }
        }

        /// <summary>
        /// Om betalingen kan gennemføres
        /// </summary>
        public bool CanCompletePayment
        {
            get { return IsPaymentMode && PaidAmount >= TotalAmount; }
        }

        // Formaterede beløb til UI
        public string TotalAmountFormatted
        {
            get { return $"{TotalAmount:C0}"; }
        }

        public string PaidAmountFormatted
        {
            get { return $"{PaidAmount:C0}"; }
        }

        public string ChangeAmountFormatted
        {
            get { return $"{ChangeAmount:C0}"; }
        }

        // Kommando properties
        public RelayCommand ScanBarcodeCommand { get; private set; }
        public RelayCommand RemoveProductCommand { get; private set; }
        public RelayCommand ProceedToPaymentCommand { get; private set; }
        public RelayCommand CompletePaymentCommand { get; private set; }
        public RelayCommand CancelSaleCommand { get; private set; }
        public RelayCommand StartNewSaleCommand { get; private set; }
        public RelayCommand CloseCommand { get; private set; }

        // Private metoder

        /// <summary>
        /// Starter et nyt salg
        /// </summary>
        private void StartNewSale()
        {
            CurrentSale = _saleService.StartNewSale();
            ScannedProducts.Clear();
            TotalAmount = 0;
            PaidAmount = 0;
            ChangeAmount = 0;
            IsPaymentMode = false;
            BarcodeInput = "";
            PaymentMethod = "Kontant";
        }

        /// <summary>
        /// Beregner byttepenge
        /// </summary>
        private void CalculateChange()
        {
            ChangeAmount = PaidAmount - TotalAmount;
        }

        /// <summary>
        /// Opdaterer total beløb fra salget
        /// </summary>
        private void UpdateTotalAmount()
        {
            if (CurrentSale != null)
            {
                TotalAmount = CurrentSale.Total;
            }
        }

        /// <summary>
        /// Opretter alle kommandoer
        /// </summary>
        private void CreateCommands()
        {
            ScanBarcodeCommand = new RelayCommand(ScanBarcode, CanExecuteScanBarcode);
            RemoveProductCommand = new RelayCommand(RemoveProduct);
            ProceedToPaymentCommand = new RelayCommand(ProceedToPayment, CanExecuteProceedToPayment);
            CompletePaymentCommand = new RelayCommand(CompletePayment, CanExecuteCompletePayment);
            CancelSaleCommand = new RelayCommand(CancelSale);
            StartNewSaleCommand = new RelayCommand(StartNewSaleFromCommand);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        // Kommando metoder

        /// <summary>
        /// Scanner en stregkode og tilføjer produktet (UC3.1 hovedfunktion)
        /// </summary>
        private void ScanBarcode(object parameter)
        {
            if (CurrentSale == null || string.IsNullOrEmpty(BarcodeInput))
                return;

            var result = _saleService.ScanBarcode(CurrentSale, BarcodeInput);

            if (result.Success)
            {
                // Opdater liste over produkter
                ScannedProducts.Clear();
                foreach (var line in CurrentSale.SaleLines)
                {
                    ScannedProducts.Add(line);
                }

                UpdateTotalAmount();
                BarcodeInput = ""; // Ryd scanner input
                StatusMessage = result.Message;
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Scanner fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool CanExecuteScanBarcode(object parameter)
        {
            return CanScanBarcode;
        }

        /// <summary>
        /// Fjerner et produkt fra salget
        /// </summary>
        private void RemoveProduct(object parameter)
        {
            if (parameter is SaleLine saleLine && CurrentSale != null)
            {
                var success = _saleService.RemoveProductFromSale(CurrentSale, saleLine);

                if (success)
                {
                    ScannedProducts.Remove(saleLine);
                    UpdateTotalAmount();
                    StatusMessage = "Produkt fjernet fra salget";
                }
            }
        }

        /// <summary>
        /// Går til betalings mode
        /// </summary>
        private void ProceedToPayment(object parameter)
        {
            if (CurrentSale == null || !HasProducts)
                return;

            IsPaymentMode = true;
            StatusMessage = $"Total: {TotalAmountFormatted} - indtast betalt beløb";

            // Hvis MobilePay, sæt betalt beløb til præcis det rigtige
            if (PaymentMethod == "MobilePay")
            {
                PaidAmount = TotalAmount;
            }
        }

        private bool CanExecuteProceedToPayment(object parameter)
        {
            return CanProceedToPayment;
        }

        /// <summary>
        /// Gennemfører betalingen (når Aya har betalt)
        /// </summary>
        private void CompletePayment(object parameter)
        {
            if (CurrentSale == null)
                return;

            var result = _saleService.ProcessPayment(CurrentSale, PaidAmount, PaymentMethod);

            if (result.Success)
            {
                string message = $"Betaling gennemført!\n\n" +
                               $"Total: {TotalAmountFormatted}\n" +
                               $"Betalt: {PaidAmountFormatted}\n" +
                               $"Byttepenge: {result.ByttePenge:C0}\n\n" +
                               $"Betalingsform: {PaymentMethod}";

                MessageBox.Show(message, "Salg gennemført", MessageBoxButton.OK, MessageBoxImage.Information);

                StatusMessage = "Salg gennemført - klar til næste kunde";

                // Start automatisk nyt salg
                StartNewSale();
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Betalingsfejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteCompletePayment(object parameter)
        {
            return CanCompletePayment;
        }

        /// <summary>
        /// Annullerer det nuværende salg
        /// </summary>
        private void CancelSale(object parameter)
        {
            if (CurrentSale == null)
                return;

            var dialogResult = MessageBox.Show("Er du sikker på at du vil annullere salget?",
                "Annuller salg", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                _saleService.CancelSale(CurrentSale);
                StartNewSale();
                StatusMessage = "Salg annulleret";
            }
        }

        /// <summary>
        /// Starter nyt salg (kommando wrapper)
        /// </summary>
        private void StartNewSaleFromCommand(object parameter)
        {
            StartNewSale();
            StatusMessage = "Nyt salg startet - scan første produkt";
        }

        /// <summary>
        /// Lukker scanner vinduet
        /// </summary>
        private void CloseWindow(object parameter)
        {
            Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.GetType().Name == "ScannerWindow")?.Close();
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}