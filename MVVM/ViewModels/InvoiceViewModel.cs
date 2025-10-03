using System;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    public class InvoiceViewModel : ViewModelBase
    {
        private readonly InvoiceService _invoiceService;

        private ObservableCollection<Invoice> _monthlyInvoices;
        private ObservableCollection<Invoice> _payoutInvoices;
        private ObservableCollection<Invoice> _billInvoices;
        private ObservableCollection<Invoice> _zeroAmountInvoices;
        private Invoice _selectedInvoice;
        private int _selectedYear;
        private int _selectedMonth;
        private string _statusMessage = string.Empty;

        private decimal _totalRevenue;
        private decimal _totalCommission;
        private decimal _totalRent;
        private decimal _totalPayouts;
        private decimal _totalBills;
        private bool _isProcessing;

        // RETTET: Parameterløs konstruktør med ServiceLocator
        public InvoiceViewModel()
        {
            // Hent service fra ServiceLocator
            _invoiceService = ServiceLocator.InvoiceService;

            _monthlyInvoices = new ObservableCollection<Invoice>();
            _payoutInvoices = new ObservableCollection<Invoice>();
            _billInvoices = new ObservableCollection<Invoice>();
            _zeroAmountInvoices = new ObservableCollection<Invoice>();

            var lastMonth = DateTime.Now.AddMonths(-1);
            SelectedYear = lastMonth.Year;
            SelectedMonth = lastMonth.Month;

            CreateCommands();
            LoadInvoicesForPeriod();
            StatusMessage = "Klar til månedlig afregning";
        }

        #region Properties

        public ObservableCollection<Invoice> MonthlyInvoices
        {
            get => _monthlyInvoices;
            set
            {
                if (SetProperty(ref _monthlyInvoices, value))
                    OnPropertyChanged(nameof(HasInvoices));
            }
        }

        public ObservableCollection<Invoice> PayoutInvoices
        {
            get => _payoutInvoices;
            set
            {
                if (SetProperty(ref _payoutInvoices, value))
                    OnPropertyChanged(nameof(HasPayouts));
            }
        }

        public ObservableCollection<Invoice> BillInvoices
        {
            get => _billInvoices;
            set
            {
                if (SetProperty(ref _billInvoices, value))
                    OnPropertyChanged(nameof(HasBills));
            }
        }

        public ObservableCollection<Invoice> ZeroAmountInvoices
        {
            get => _zeroAmountInvoices;
            set
            {
                if (SetProperty(ref _zeroAmountInvoices, value))
                    OnPropertyChanged(nameof(HasZeroAmount));
            }
        }

        public Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                if (SetProperty(ref _selectedInvoice, value))
                    OnPropertyChanged(nameof(IsInvoiceSelected));
            }
        }

        public int SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value))
                    OnPropertyChanged(nameof(PeriodDescription));
            }
        }

        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value))
                    OnPropertyChanged(nameof(PeriodDescription));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                if (SetProperty(ref _totalRevenue, value))
                    OnPropertyChanged(nameof(TotalRevenueFormatted));
            }
        }

        public decimal TotalCommission
        {
            get => _totalCommission;
            set
            {
                if (SetProperty(ref _totalCommission, value))
                    OnPropertyChanged(nameof(TotalCommissionFormatted));
            }
        }

        public decimal TotalRent
        {
            get => _totalRent;
            set
            {
                if (SetProperty(ref _totalRent, value))
                    OnPropertyChanged(nameof(TotalRentFormatted));
            }
        }

        public decimal TotalPayouts
        {
            get => _totalPayouts;
            set
            {
                if (SetProperty(ref _totalPayouts, value))
                    OnPropertyChanged(nameof(TotalPayoutsFormatted));
            }
        }

        public decimal TotalBills
        {
            get => _totalBills;
            set
            {
                if (SetProperty(ref _totalBills, value))
                    OnPropertyChanged(nameof(TotalBillsFormatted));
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                    OnPropertyChanged(nameof(CanProcessPeriod));
            }
        }

        #endregion

        #region Computed Properties

        public bool HasInvoices => MonthlyInvoices != null && MonthlyInvoices.Count > 0;
        public bool HasPayouts => PayoutInvoices != null && PayoutInvoices.Count > 0;
        public bool HasBills => BillInvoices != null && BillInvoices.Count > 0;
        public bool HasZeroAmount => ZeroAmountInvoices != null && ZeroAmountInvoices.Count > 0;
        public bool IsInvoiceSelected => SelectedInvoice != null;
        public bool CanProcessPeriod => !IsProcessing;
        public string PeriodDescription => $"{GetMonthName(SelectedMonth)} {SelectedYear}";

        public string TotalRevenueFormatted => TotalRevenue.ToString("C0");
        public string TotalCommissionFormatted => TotalCommission.ToString("C0");
        public string TotalRentFormatted => TotalRent.ToString("C0");
        public string TotalPayoutsFormatted => TotalPayouts.ToString("C0");
        public string TotalBillsFormatted => TotalBills.ToString("C0");

        public decimal NetStoreResult => TotalCommission - TotalBills;
        public string NetStoreResultFormatted => NetStoreResult.ToString("C0");

        #endregion

        #region Commands

        public RelayCommand CreateMonthlyInvoicesCommand { get; private set; }
        public RelayCommand LoadPeriodCommand { get; private set; }
        public RelayCommand ProcessPayoutCommand { get; private set; }
        public RelayCommand SendBillCommand { get; private set; }
        public RelayCommand RefreshDataCommand { get; private set; }

        #endregion

        #region Methods

        private void LoadInvoicesForPeriod()
        {
            MonthlyInvoices = new ObservableCollection<Invoice>(
                _invoiceService.GetInvoicesForPeriod(SelectedYear, SelectedMonth));

            PayoutInvoices = new ObservableCollection<Invoice>(
                _invoiceService.GetInvoicesForPayout());

            BillInvoices = new ObservableCollection<Invoice>(
                _invoiceService.GetInvoicesForBilling());

            ZeroAmountInvoices = new ObservableCollection<Invoice>(
                _invoiceService.GetInvoicesWithZeroAmount());

            TotalRevenue = _invoiceService.CalculateTotalRevenue(SelectedYear, SelectedMonth);
            TotalCommission = _invoiceService.CalculateTotalCommission(SelectedYear, SelectedMonth);
            TotalRent = _invoiceService.CalculateTotalRent(SelectedYear, SelectedMonth);
            TotalPayouts = _invoiceService.CalculateTotalPendingPayouts();
            TotalBills = _invoiceService.CalculateTotalPendingCharges();

            StatusMessage = $"Indlæst {MonthlyInvoices.Count} fakturaer for {PeriodDescription}";
        }

        private void CreateCommands()
        {
            CreateMonthlyInvoicesCommand = new RelayCommand(CreateMonthlyInvoices, CanExecuteProcess);
            LoadPeriodCommand = new RelayCommand(LoadPeriod);
            ProcessPayoutCommand = new RelayCommand(ProcessPayout);
            SendBillCommand = new RelayCommand(SendBill);
            RefreshDataCommand = new RelayCommand(RefreshData);
        }

        private bool CanExecuteProcess(object parameter) => CanProcessPeriod;

        private void CreateMonthlyInvoices(object parameter)
        {
            IsProcessing = true;
            StatusMessage = "Opretter fakturaer...";

            var result = _invoiceService.CreateMonthlyInvoices(SelectedYear, SelectedMonth);
            StatusMessage = result.Success
                ? $"Fakturaer oprettet! Antal: {result.CreatedInvoices.Count}"
                : $"Fejl: {result.ErrorMessage}";

            LoadInvoicesForPeriod();
            IsProcessing = false;
        }

        private void LoadPeriod(object parameter) => LoadInvoicesForPeriod();

        private void ProcessPayout(object parameter)
        {
            if (parameter is Invoice invoice)
            {
                var result = _invoiceService.ProcessInvoicePayment(invoice.InvoiceId, "Bankoverførsel");
                StatusMessage = result.Success ? result.Message : $"Fejl: {result.ErrorMessage}";
                LoadInvoicesForPeriod();
            }
        }

        private void SendBill(object parameter)
        {
            if (parameter is Invoice invoice)
            {
                var result = _invoiceService.ProcessInvoicePayment(invoice.InvoiceId, "Regning sendt");
                StatusMessage = result.Success ? result.Message : $"Fejl: {result.ErrorMessage}";
                LoadInvoicesForPeriod();
            }
        }

        private void RefreshData(object parameter) => LoadInvoicesForPeriod();

        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };
            return (month >= 1 && month <= 12) ? monthNames[month] : "Ukendt";
        }

        #endregion
    }
}