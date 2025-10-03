using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    public class ScannerViewModel : ViewModelBase
    {
        private readonly SaleService _saleService;

        private int _currentSaleId;
        private ObservableCollection<SaleLineViewModel> _scannedProducts;
        private string _barcodeInput = string.Empty;
        private decimal _totalAmount;
        private decimal _paidAmount;
        private decimal _changeAmount;
        private PaymentMethod _paymentMethod = PaymentMethod.Cash;
        private string _statusMessage = string.Empty;
        private bool _isPaymentMode;

        public ScannerViewModel()
        {
            _saleService = ServiceLocator.SaleService;
            _scannedProducts = new ObservableCollection<SaleLineViewModel>();

            StartNewSale();
            CreateCommands();
            StatusMessage = "Scanner klar - scan første produkt";
        }

        public ObservableCollection<SaleLineViewModel> ScannedProducts
        {
            get => _scannedProducts;
            set => SetProperty(ref _scannedProducts, value);
        }

        public string BarcodeInput
        {
            get => _barcodeInput;
            set => SetProperty(ref _barcodeInput, value);
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (SetProperty(ref _totalAmount, value))
                {
                    OnPropertyChanged(nameof(TotalAmountFormatted));
                    CalculateChange();
                }
            }
        }

        public decimal PaidAmount
        {
            get => _paidAmount;
            set
            {
                if (SetProperty(ref _paidAmount, value))
                {
                    OnPropertyChanged(nameof(PaidAmountFormatted));
                    OnPropertyChanged(nameof(CanCompletePayment));
                    CalculateChange();
                }
            }
        }

        public decimal ChangeAmount
        {
            get => _changeAmount;
            set
            {
                if (SetProperty(ref _changeAmount, value))
                    OnPropertyChanged(nameof(ChangeAmountFormatted));
            }
        }

        public PaymentMethod PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsPaymentMode
        {
            get => _isPaymentMode;
            set
            {
                if (SetProperty(ref _isPaymentMode, value))
                    OnPropertyChanged(nameof(IsScanningMode));
            }
        }

        public bool HasProducts => ScannedProducts.Any();
        public bool IsScanningMode => !IsPaymentMode;
        public int ProductCount => ScannedProducts.Count;
        public bool CanScanBarcode => IsScanningMode;
        public bool CanProceedToPayment => HasProducts && IsScanningMode;
        public bool CanCompletePayment => IsPaymentMode && PaidAmount >= TotalAmount;

        public string TotalAmountFormatted => $"{TotalAmount:C0}";
        public string PaidAmountFormatted => $"{PaidAmount:C0}";
        public string ChangeAmountFormatted => $"{ChangeAmount:C0}";

        public RelayCommand ScanBarcodeCommand { get; private set; }
        public RelayCommand RemoveProductCommand { get; private set; }
        public RelayCommand ProceedToPaymentCommand { get; private set; }
        public RelayCommand CompletePaymentCommand { get; private set; }
        public RelayCommand CancelSaleCommand { get; private set; }
        public RelayCommand StartNewSaleCommand { get; private set; }

        private void StartNewSale()
        {
            var sale = _saleService.StartNewSale();
            _currentSaleId = sale.SaleId;
            ScannedProducts.Clear();
            TotalAmount = 0;
            PaidAmount = 0;
            ChangeAmount = 0;
            IsPaymentMode = false;
            BarcodeInput = string.Empty;
            PaymentMethod = PaymentMethod.Cash;
        }

        private void CalculateChange()
        {
            ChangeAmount = PaidAmount - TotalAmount;
        }

        private void CreateCommands()
        {
            ScanBarcodeCommand = new RelayCommand(ScanBarcode, _ => CanScanBarcode);
            RemoveProductCommand = new RelayCommand(RemoveProduct);
            ProceedToPaymentCommand = new RelayCommand(ProceedToPayment, _ => CanProceedToPayment);
            CompletePaymentCommand = new RelayCommand(CompletePayment, _ => CanCompletePayment);
            CancelSaleCommand = new RelayCommand(CancelSale);
            StartNewSaleCommand = new RelayCommand(StartNewSaleFromCommand);
        }

        private void ScanBarcode(object parameter)
        {
            if (string.IsNullOrEmpty(BarcodeInput))
            {
                StatusMessage = "Ingen stregkode indtastet";
                return;
            }

            var result = _saleService.ScanBarcode(_currentSaleId, BarcodeInput);

            if (result.Success)
            {
                var saleLineVM = new SaleLineViewModel(result.AddedSaleLine);
                ScannedProducts.Add(saleLineVM);

                var sale = _saleService.GetSaleById(_currentSaleId);
                TotalAmount = sale.Total;

                BarcodeInput = string.Empty;
                StatusMessage = result.Message;
                OnPropertyChanged(nameof(HasProducts));
                OnPropertyChanged(nameof(ProductCount));
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Scanner fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoveProduct(object parameter)
        {
            if (parameter is SaleLineViewModel saleLineVM)
            {
                _saleService.RemoveProductFromSale(_currentSaleId, saleLineVM.SaleLineId);
                ScannedProducts.Remove(saleLineVM);

                var sale = _saleService.GetSaleById(_currentSaleId);
                TotalAmount = sale.Total;

                StatusMessage = "Produkt fjernet";
                OnPropertyChanged(nameof(HasProducts));
                OnPropertyChanged(nameof(ProductCount));
            }
        }

        private void ProceedToPayment(object parameter)
        {
            IsPaymentMode = true;
            StatusMessage = $"Total: {TotalAmountFormatted} - indtast betalt beløb";

            if (PaymentMethod == PaymentMethod.MobilePay)
                PaidAmount = TotalAmount;
        }

        private void CompletePayment(object parameter)
        {
            var result = _saleService.ProcessPayment(_currentSaleId, PaidAmount, PaymentMethod);

            if (result.Success)
            {
                MessageBox.Show(
                    $"Betaling gennemført!\n\nTotal: {TotalAmountFormatted}\nBetalt: {PaidAmountFormatted}\nByttepenge: {result.ChangeGiven:C0}",
                    "Salg gennemført",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StatusMessage = "Salg gennemført";
                StartNewSale();
            }
            else
            {
                StatusMessage = result.ErrorMessage;
                MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelSale(object parameter)
        {
            var dialogResult = MessageBox.Show(
                "Er du sikker på at du vil annullere salget?",
                "Annuller salg",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (dialogResult == MessageBoxResult.Yes)
            {
                _saleService.CancelSale(_currentSaleId);
                StartNewSale();
                StatusMessage = "Salg annulleret";
            }
        }

        private void StartNewSaleFromCommand(object parameter)
        {
            StartNewSale();
            StatusMessage = "Nyt salg startet";
        }
    }
}