using ReolMarked.MVVM.Commands;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;
using ReolMarkedTest1.MVVM.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// Step 3: Opdateret FakturaViewModel med korrekte beregninger og visning
    /// Følger Jonas og Sofies månedopgørelse proces
    /// </summary>
    public class FakturaViewModel : INotifyPropertyChanged
    {
        // Private felter til services
        private readonly FakturaService _fakturaService;
        private readonly CustomerRepository _customerRepository;
        private readonly RackRepository _rackRepository;
        private readonly RentalService _rentalService;
        private readonly SaleService _saleService;
        private readonly BarcodeService _barcodeService;

        // Private felter til UI binding
        private ObservableCollection<Faktura> _monthlyFakturaer = new();
        private ObservableCollection<Faktura> _payoutFakturaer = new();
        private ObservableCollection<Faktura> _billFakturaer = new();
        private ObservableCollection<Faktura> _zeroAmountFakturaer = new(); // NYE - lige op fakturaer
        private Faktura _selectedFaktura;
        private int _selectedYear;
        private int _selectedMonth;
        private string _statusMessage = "";

        // OPDATEREDE totaler med reolleje
        private decimal _totalRevenue;
        private decimal _totalCommission;
        private decimal _totalRent;           // NYE - total reolleje
        private decimal _totalPayouts;
        private decimal _totalBills;
        private bool _isProcessing;

        // Konstruktør - opsætter services
        public FakturaViewModel()
        {
            // Opret repositories og services
            _rackRepository = new RackRepository();
            _customerRepository = new CustomerRepository();
            _rentalService = new RentalService(_customerRepository, _rackRepository);
            _barcodeService = new BarcodeService(_customerRepository, _rackRepository, _rentalService);
            _saleService = new SaleService(_barcodeService);
            _fakturaService = new FakturaService(_customerRepository, _rackRepository, _rentalService, _saleService);

            // Sæt default periode til forrige måned
            var lastMonth = DateTime.Now.AddMonths(-1);
            SelectedYear = lastMonth.Year;
            SelectedMonth = lastMonth.Month;

            // Opret kommandoer
            CreateCommands();

            // Indlæs data for default periode
            LoadFakturaerForPeriod();

            // Sæt initial status
            StatusMessage = "Klar til månedlig afregning";
        }

        // Properties til UI binding

        /// <summary>
        /// Liste over fakturaer for valgte måned
        /// </summary>
        public ObservableCollection<Faktura> MonthlyFakturaer
        {
            get { return _monthlyFakturaer; }
            set
            {
                _monthlyFakturaer = value;
                OnPropertyChanged(nameof(MonthlyFakturaer));
                OnPropertyChanged(nameof(HasFakturaer));
            }
        }

        /// <summary>
        /// Fakturaer der kræver udbetaling (positive beløb)
        /// </summary>
        public ObservableCollection<Faktura> PayoutFakturaer
        {
            get { return _payoutFakturaer; }
            set
            {
                _payoutFakturaer = value;
                OnPropertyChanged(nameof(PayoutFakturaer));
                OnPropertyChanged(nameof(HasPayouts));
            }
        }

        /// <summary>
        /// Fakturaer der kræver regning (negative beløb - "røde tal")
        /// </summary>
        public ObservableCollection<Faktura> BillFakturaer
        {
            get { return _billFakturaer; }
            set
            {
                _billFakturaer = value;
                OnPropertyChanged(nameof(BillFakturaer));
                OnPropertyChanged(nameof(HasBills));
            }
        }

        /// <summary>
        /// NYE - Fakturaer der er lige op (nul beløb)
        /// </summary>
        public ObservableCollection<Faktura> ZeroAmountFakturaer
        {
            get { return _zeroAmountFakturaer; }
            set
            {
                _zeroAmountFakturaer = value;
                OnPropertyChanged(nameof(ZeroAmountFakturaer));
                OnPropertyChanged(nameof(HasZeroAmount));
            }
        }

        /// <summary>
        /// Valgt faktura til detaljer
        /// </summary>
        public Faktura SelectedFaktura
        {
            get { return _selectedFaktura; }
            set
            {
                _selectedFaktura = value;
                OnPropertyChanged(nameof(SelectedFaktura));
                OnPropertyChanged(nameof(IsFakturaSelected));
            }
        }

        /// <summary>
        /// Valgt år for afregning
        /// </summary>
        public int SelectedYear
        {
            get { return _selectedYear; }
            set
            {
                _selectedYear = value;
                OnPropertyChanged(nameof(SelectedYear));
                OnPropertyChanged(nameof(PeriodDescription));
            }
        }

        /// <summary>
        /// Valgt måned for afregning
        /// </summary>
        public int SelectedMonth
        {
            get { return _selectedMonth; }
            set
            {
                _selectedMonth = value;
                OnPropertyChanged(nameof(SelectedMonth));
                OnPropertyChanged(nameof(PeriodDescription));
            }
        }

        /// <summary>
        /// Status besked til Jonas og Sofie
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
        /// Total omsætning for perioden
        /// </summary>
        public decimal TotalRevenue
        {
            get { return _totalRevenue; }
            set
            {
                _totalRevenue = value;
                OnPropertyChanged(nameof(TotalRevenue));
                OnPropertyChanged(nameof(TotalRevenueFormatted));
            }
        }

        /// <summary>
        /// Total kommission for perioden
        /// </summary>
        public decimal TotalCommission
        {
            get { return _totalCommission; }
            set
            {
                _totalCommission = value;
                OnPropertyChanged(nameof(TotalCommission));
                OnPropertyChanged(nameof(TotalCommissionFormatted));
            }
        }

        /// <summary>
        /// NYE - Total reolleje for perioden
        /// </summary>
        public decimal TotalRent
        {
            get { return _totalRent; }
            set
            {
                _totalRent = value;
                OnPropertyChanged(nameof(TotalRent));
                OnPropertyChanged(nameof(TotalRentFormatted));
            }
        }

        /// <summary>
        /// Total udbetalinger
        /// </summary>
        public decimal TotalPayouts
        {
            get { return _totalPayouts; }
            set
            {
                _totalPayouts = value;
                OnPropertyChanged(nameof(TotalPayouts));
                OnPropertyChanged(nameof(TotalPayoutsFormatted));
            }
        }

        /// <summary>
        /// Total regninger ("røde tal")
        /// </summary>
        public decimal TotalBills
        {
            get { return _totalBills; }
            set
            {
                _totalBills = value;
                OnPropertyChanged(nameof(TotalBills));
                OnPropertyChanged(nameof(TotalBillsFormatted));
            }
        }

        /// <summary>
        /// Om der behandles data
        /// </summary>
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                _isProcessing = value;
                OnPropertyChanged(nameof(IsProcessing));
                OnPropertyChanged(nameof(CanProcessPeriod));
            }
        }

        // Beregnet properties til UI kontrol

        /// <summary>
        /// Om der er fakturaer for perioden
        /// </summary>
        public bool HasFakturaer
        {
            get { return MonthlyFakturaer.Count > 0; }
        }

        /// <summary>
        /// Om der er udbetalinger
        /// </summary>
        public bool HasPayouts
        {
            get { return PayoutFakturaer.Count > 0; }
        }

        /// <summary>
        /// Om der er regninger
        /// </summary>
        public bool HasBills
        {
            get { return BillFakturaer.Count > 0; }
        }

        /// <summary>
        /// NYE - Om der er lige op fakturaer
        /// </summary>
        public bool HasZeroAmount
        {
            get { return ZeroAmountFakturaer.Count > 0; }
        }

        /// <summary>
        /// Om der er valgt en faktura
        /// </summary>
        public bool IsFakturaSelected
        {
            get { return SelectedFaktura != null; }
        }

        /// <summary>
        /// Om der kan behandles en periode
        /// </summary>
        public bool CanProcessPeriod
        {
            get { return !IsProcessing; }
        }

        /// <summary>
        /// Beskrivelse af valgte periode
        /// </summary>
        public string PeriodDescription
        {
            get { return $"{GetMonthName(SelectedMonth)} {SelectedYear}"; }
        }

        // Formaterede værdier til UI
        public string TotalRevenueFormatted
        {
            get { return $"{TotalRevenue:C0}"; }
        }

        public string TotalCommissionFormatted
        {
            get { return $"{TotalCommission:C0}"; }
        }

        public string TotalRentFormatted
        {
            get { return $"{TotalRent:C0}"; }
        }

        public string TotalPayoutsFormatted
        {
            get { return $"{TotalPayouts:C0}"; }
        }

        public string TotalBillsFormatted
        {
            get { return $"{TotalBills:C0}"; }
        }

        /// <summary>
        /// NYE - Netto resultat for butikken (kommission - eventuelle underskud)
        /// </summary>
        public decimal NetStoreResult
        {
            get { return TotalCommission - TotalBills; }
        }

        public string NetStoreResultFormatted
        {
            get { return $"{NetStoreResult:C0}"; }
        }

        // Kommando properties
        public RelayCommand CreateMonthlyFakturaerCommand { get; private set; }
        public RelayCommand LoadPeriodCommand { get; private set; }
        public RelayCommand ProcessPayoutCommand { get; private set; }
        public RelayCommand SendBillCommand { get; private set; }
        public RelayCommand RefreshDataCommand { get; private set; }

        // Private metoder

        /// <summary>
        /// OPDATERET - Indlæser fakturaer for valgte periode med alle kategorier
        /// </summary>
        private void LoadFakturaerForPeriod()
        {
            MonthlyFakturaer = _fakturaService.GetFakturaerForPeriod(SelectedYear, SelectedMonth);
            PayoutFakturaer = _fakturaService.GetFakturaerForUdbetaling();
            BillFakturaer = _fakturaService.GetFakturaerForOpkraevning();
            ZeroAmountFakturaer = _fakturaService.GetFakturaerLigeOp(); // NYE

            // OPDATEREDE beregninger med reolleje
            TotalRevenue = _fakturaService.CalculateTotalRevenue(SelectedYear, SelectedMonth);
            TotalCommission = _fakturaService.CalculateTotalCommission(SelectedYear, SelectedMonth);
            TotalRent = _fakturaService.CalculateTotalRent(SelectedYear, SelectedMonth); // NYE
            TotalPayouts = _fakturaService.CalculateTotalPendingPayouts();
            TotalBills = _fakturaService.CalculateTotalPendingCharges();

            StatusMessage = $"Indlæst {MonthlyFakturaer.Count} fakturaer for {PeriodDescription}";
        }

        /// <summary>
        /// Opretter alle kommandoer
        /// </summary>
        private void CreateCommands()
        {
            CreateMonthlyFakturaerCommand = new RelayCommand(CreateMonthlyFakturaer, CanExecuteProcessPeriod);
            LoadPeriodCommand = new RelayCommand(LoadPeriod);
            ProcessPayoutCommand = new RelayCommand(ProcessPayout);
            SendBillCommand = new RelayCommand(SendBill);
            RefreshDataCommand = new RelayCommand(RefreshData);
        }

        // Kommando metoder

        /// <summary>
        /// OPDATERET - Opretter månedlige fakturaer med forbedret feedback
        /// </summary>
        private void CreateMonthlyFakturaer(object parameter)
        {
            IsProcessing = true;
            StatusMessage = "Opretter månedlige fakturaer...";

            try
            {
                var result = _fakturaService.CreateMonthlyFakturaer(SelectedYear, SelectedMonth);

                if (result.Success)
                {
                    LoadFakturaerForPeriod();

                    // OPDATERET besked med alle beregninger
                    string message = $"Månedlige fakturaer oprettet!\n\n" +
                                   $"Periode: {PeriodDescription}\n" +
                                   $"Antal fakturaer: {result.CreatedFakturaer.Count}\n\n" +
                                   $"TOTALER:\n" +
                                   $"Omsætning: {TotalRevenueFormatted}\n" +
                                   $"Kommission (10%): {TotalCommissionFormatted}\n" +
                                   $"Reolleje: {TotalRentFormatted}\n\n" +
                                   $"HANDLINGER:\n" +
                                   $"Udbetalinger: {PayoutFakturaer.Count} kunder ({TotalPayoutsFormatted})\n" +
                                   $"Regninger: {BillFakturaer.Count} kunder ({TotalBillsFormatted})\n" +
                                   $"Lige op: {ZeroAmountFakturaer.Count} kunder\n\n" +
                                   $"Butikkens netto: {NetStoreResultFormatted}";

                    MessageBox.Show(message, "Fakturaer oprettet", MessageBoxButton.OK, MessageBoxImage.Information);
                    StatusMessage = result.Message;
                }
                else
                {
                    StatusMessage = result.ErrorMessage;
                    MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool CanExecuteProcessPeriod(object parameter)
        {
            return CanProcessPeriod;
        }

        /// <summary>
        /// Indlæser data for valgte periode
        /// </summary>
        private void LoadPeriod(object parameter)
        {
            LoadFakturaerForPeriod();
        }

        /// <summary>
        /// Behandler udbetaling til kunde
        /// </summary>
        private void ProcessPayout(object parameter)
        {
            if (parameter is Faktura faktura && faktura.IsPositiveAmount)
            {
                var result = _fakturaService.ProcessFakturaPayment(faktura, "Bankoverførsel");

                if (result.Success)
                {
                    StatusMessage = $"Udbetaling til {faktura.CustomerName}: {faktura.NetAmountFormatted}";
                    MessageBox.Show($"Udbetaling gennemført!\n\nKunde: {faktura.CustomerName}\nBeløb: {faktura.NetAmountFormatted}",
                        "Udbetaling", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadFakturaerForPeriod();
                }
                else
                {
                    MessageBox.Show(result.ErrorMessage, "Fejl", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Sender regning til kunde
        /// </summary>
        private void SendBill(object parameter)
        {
            if (parameter is Faktura faktura && faktura.IsNegativeAmount)
            {
                var result = _fakturaService.ProcessFakturaPayment(faktura, "Regning sendt");

                if (result.Success)
                {
                    StatusMessage = $"Regning sendt til {faktura.CustomerName}: {Math.Abs(faktura.NetAmount):C0}";
                    MessageBox.Show($"Regning sendt!\n\nKunde: {faktura.CustomerName}\nBeløb: {Math.Abs(faktura.NetAmount):C0}",
                        "Regning", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadFakturaerForPeriod();
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
            LoadFakturaerForPeriod();
            StatusMessage = "Data genindlæst";
        }

        /// <summary>
        /// Hjælpe metode til måned navne
        /// </summary>
        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };

            return month >= 1 && month <= 12 ? monthNames[month] : "Ukendt";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}