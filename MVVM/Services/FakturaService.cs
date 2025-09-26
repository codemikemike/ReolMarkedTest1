using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;
using ReolMarked.MVVM.Models;

namespace ReolMarkedTest1.MVVM.Services
{
    /// <summary>
    /// Step 2: Opdateret FakturaService med korrekt beregningslogik
    /// Følger Jonas og Sofies manuel proces fra scenariet
    /// </summary>
    public class FakturaService
    {
        // Private lister til at gemme fakturaer
        private List<Faktura> _fakturaer;
        private int _nextFakturaId;

        // Reference til andre services og repositories
        private CustomerRepository _customerRepository;
        private RackRepository _rackRepository;
        private RentalService _rentalService;
        private SaleService _saleService;

        public FakturaService(CustomerRepository customerRepository, RackRepository rackRepository,
                             RentalService rentalService, SaleService saleService)
        {
            _fakturaer = new List<Faktura>();
            _nextFakturaId = 1;
            _customerRepository = customerRepository;
            _rackRepository = rackRepository;
            _rentalService = rentalService;
            _saleService = saleService;
        }

        /// <summary>
        /// OPDATERET: Opretter en faktura for en specifik kunde med korrekt beregning
        /// Følger Jonas og Sofies proces: Salg - Kommission - Næste måneds leje
        /// </summary>
        public Faktura CreateFakturaForCustomer(Customer customer, int year, int month)
        {
            if (customer == null)
                return null;

            // Opret basis faktura
            var faktura = Faktura.CreateMonthlyFaktura(customer, year, month);
            faktura.FakturaId = _nextFakturaId++;

            // 1. Find kundens reoler (som Jonas finder kortene)
            var customerRacks = _rentalService.GetRacksForCustomer(customer.CustomerId);
            if (customerRacks.Count == 0)
            {
                // Kunde har ingen reoler - skip
                return null;
            }
            faktura.CustomerRacks = customerRacks;

            // 2. Hent alle salg for kunden i perioden (som Jonas summerer fra kortene)
            var rackSales = GetRackSalesForCustomerInPeriod(customer.CustomerId,
                faktura.PeriodStart, faktura.PeriodEnd);

            // 3. Beregn total salg (som Jonas summerer tallene)
            faktura.TotalSales = CalculateTotalSales(rackSales);

            // 4. Beregn 10% kommission (som Jonas beregner kommissionen)
            faktura.CalculateCommission();

            // 5. Beregn næste måneds reolleje (som Jonas trækker næste måneds leje)
            faktura.NextMonthRent = CalculateNextMonthRent(customer.CustomerId);

            // 6. Beregn slutbeløb med korrekt formel
            faktura.CalculateNetAmount();

            // Tilføj detaljer
            faktura.RackSales = new ObservableCollection<RackSale>(rackSales);
            var rentalAgreements = _rentalService.GetActiveAgreementsForCustomer(customer.CustomerId);
            faktura.RentalAgreements = rentalAgreements;

            // Marker som færdig
            faktura.CompleteFaktura();

            // Tilføj til listen
            _fakturaer.Add(faktura);

            return faktura;
        }

        /// <summary>
        /// NYE: Beregner næste måneds reolleje for en kunde
        /// Dette var manglende i original implementering
        /// </summary>
        private decimal CalculateNextMonthRent(int customerId)
        {
            var agreements = _rentalService.GetActiveAgreementsForCustomer(customerId);
            decimal totalRent = 0;

            foreach (var agreement in agreements)
            {
                if (agreement.IsActive)
                {
                    totalRent += agreement.Price; // Månedlig leje per reol
                }
            }

            return totalRent;
        }

        /// <summary>
        /// Opretter månedlige fakturaer for alle kunder (UC4.1 hovedproces)
        /// </summary>
        public FakturaGenerationResult CreateMonthlyFakturaer(int year, int month)
        {
            var result = new FakturaGenerationResult();
            var createdFakturaer = new List<Faktura>();

            // Tjek om der allerede er oprettet fakturaer for denne periode
            var existingFakturaer = GetFakturaerForPeriod(year, month);
            if (existingFakturaer.Count > 0)
            {
                result.Success = false;
                result.ErrorMessage = $"Der er allerede oprettet fakturaer for {GetMonthName(month)} {year}";
                return result;
            }

            // Hent alle aktive kunder med reoler
            var activeCustomers = _customerRepository.GetActiveCustomers();

            foreach (var customer in activeCustomers)
            {
                // Tjek om kunden har reoler i den givne periode
                var customerRacks = _rentalService.GetRacksForCustomer(customer.CustomerId);

                if (customerRacks.Count > 0)
                {
                    var faktura = CreateFakturaForCustomer(customer, year, month);
                    if (faktura != null)
                    {
                        createdFakturaer.Add(faktura);
                    }
                }
            }

            result.Success = true;
            result.CreatedFakturaer = new ObservableCollection<Faktura>(createdFakturaer);
            result.Message = $"Oprettet {createdFakturaer.Count} fakturaer for {GetMonthName(month)} {year}";

            return result;
        }

        /// <summary>
        /// OPDATERET: Gennemfører udbetaling (som Jonas gør ved computeren)
        /// </summary>
        public PaymentResult ProcessFakturaPayment(Faktura faktura, string paymentMethod)
        {
            var result = new PaymentResult();

            if (faktura == null || !faktura.IsCompleted)
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldig eller ikke færdig faktura";
                return result;
            }

            if (faktura.IsPaid)
            {
                result.Success = false;
                result.ErrorMessage = "Fakturaen er allerede betalt/udbetalt";
                return result;
            }

            // Håndter udbetaling (positive beløb)
            if (faktura.IsPositiveAmount)
            {
                faktura.PaymentMethod = paymentMethod;
                faktura.MarkAsPaid();

                result.Success = true;
                result.Message = $"Udbetaling på {faktura.NetAmountFormatted} til {faktura.CustomerName} gennemført";
            }
            // Håndter regning (negative beløb - "røde tal")
            else if (faktura.IsNegativeAmount)
            {
                faktura.PaymentMethod = paymentMethod;
                faktura.MarkBillSent();

                result.Success = true;
                result.Message = $"Regning på {Math.Abs(faktura.NetAmount):C0} sendt til {faktura.CustomerName}";
            }
            // Intet at gøre (nul beløb)
            else
            {
                faktura.PaymentMethod = "Ingen handling nødvendig";
                faktura.MarkAsPaid();

                result.Success = true;
                result.Message = $"{faktura.CustomerName} - lige op, ingen handling nødvendig";
            }

            return result;
        }

        /// <summary>
        /// OPDATERET: Henter fakturaer der kræver udbetaling (positive beløb)
        /// </summary>
        public ObservableCollection<Faktura> GetFakturaerForUdbetaling()
        {
            var udbetalingsFakturaer = new List<Faktura>();

            foreach (var faktura in _fakturaer)
            {
                if (faktura.IsCompleted && !faktura.IsPaid && faktura.IsPositiveAmount)
                {
                    udbetalingsFakturaer.Add(faktura);
                }
            }

            return new ObservableCollection<Faktura>(udbetalingsFakturaer);
        }

        /// <summary>
        /// OPDATERET: Henter fakturaer der kræver regning ("røde tal")
        /// </summary>
        public ObservableCollection<Faktura> GetFakturaerForOpkraevning()
        {
            var opkraevningsFakturaer = new List<Faktura>();

            foreach (var faktura in _fakturaer)
            {
                if (faktura.IsCompleted && !faktura.IsPaid && faktura.IsNegativeAmount)
                {
                    opkraevningsFakturaer.Add(faktura);
                }
            }

            return new ObservableCollection<Faktura>(opkraevningsFakturaer);
        }

        /// <summary>
        /// NYE: Henter fakturaer hvor intet skal gøres (nul beløb)
        /// </summary>
        public ObservableCollection<Faktura> GetFakturaerLigeOp()
        {
            var ligeOpFakturaer = new List<Faktura>();

            foreach (var faktura in _fakturaer)
            {
                if (faktura.IsCompleted && faktura.IsZeroAmount)
                {
                    ligeOpFakturaer.Add(faktura);
                }
            }

            return new ObservableCollection<Faktura>(ligeOpFakturaer);
        }

        /// <summary>
        /// OPDATERET: Beregner total udestående udbetalinger
        /// </summary>
        public decimal CalculateTotalPendingPayouts()
        {
            decimal total = 0;
            var udbetalingsFakturaer = GetFakturaerForUdbetaling();

            foreach (var faktura in udbetalingsFakturaer)
            {
                total += faktura.NetAmount;
            }

            return total;
        }

        /// <summary>
        /// OPDATERET: Beregner total udestående regninger
        /// </summary>
        public decimal CalculateTotalPendingCharges()
        {
            decimal total = 0;
            var opkraevningsFakturaer = GetFakturaerForOpkraevning();

            foreach (var faktura in opkraevningsFakturaer)
            {
                total += Math.Abs(faktura.NetAmount); // Positive tal for visning
            }

            return total;
        }

        // Eksisterende metoder (uændrede)
        public ObservableCollection<Faktura> GetFakturaerForPeriod(int year, int month)
        {
            var periodFakturaer = new List<Faktura>();

            foreach (var faktura in _fakturaer)
            {
                if (faktura.PeriodStart.Year == year && faktura.PeriodStart.Month == month)
                {
                    periodFakturaer.Add(faktura);
                }
            }

            return new ObservableCollection<Faktura>(periodFakturaer);
        }

        public decimal CalculateTotalRevenue(int year, int month)
        {
            decimal total = 0;
            var periodFakturaer = GetFakturaerForPeriod(year, month);

            foreach (var faktura in periodFakturaer)
            {
                total += faktura.TotalSales;
            }

            return total;
        }

        public decimal CalculateTotalCommission(int year, int month)
        {
            decimal total = 0;
            var periodFakturaer = GetFakturaerForPeriod(year, month);

            foreach (var faktura in periodFakturaer)
            {
                total += faktura.KommissionAmount;
            }

            return total;
        }

        // NYE: Beregner total reolleje for perioden
        public decimal CalculateTotalRent(int year, int month)
        {
            decimal total = 0;
            var periodFakturaer = GetFakturaerForPeriod(year, month);

            foreach (var faktura in periodFakturaer)
            {
                total += faktura.NextMonthRent;
            }

            return total;
        }

        public Faktura GetFakturaById(int fakturaId)
        {
            foreach (var faktura in _fakturaer)
            {
                if (faktura.FakturaId == fakturaId)
                {
                    return faktura;
                }
            }

            return null;
        }

        // Private hjælpemetoder
        private List<RackSale> GetRackSalesForCustomerInPeriod(int customerId, DateTime startDate, DateTime endDate)
        {
            var customerSales = new List<RackSale>();
            var allRackSales = _saleService.GetRackSalesForCustomer(customerId);

            foreach (var rackSale in allRackSales)
            {
                if (rackSale.Dato.Date >= startDate.Date && rackSale.Dato.Date <= endDate.Date)
                {
                    customerSales.Add(rackSale);
                }
            }

            return customerSales;
        }

        private decimal CalculateTotalSales(List<RackSale> rackSales)
        {
            decimal total = 0;

            foreach (var sale in rackSales)
            {
                total += sale.Belob;
            }

            return total;
        }

        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };

            return month >= 1 && month <= 12 ? monthNames[month] : "Ukendt";
        }

        /// <summary>
        /// Result klasse for faktura generering
        /// </summary>
        public class FakturaGenerationResult
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = "";
            public string Message { get; set; } = "";
            public ObservableCollection<Faktura> CreatedFakturaer { get; set; } = new();
        }
    }
}