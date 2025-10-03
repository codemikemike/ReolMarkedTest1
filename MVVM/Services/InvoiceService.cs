using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services.Results;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere fakturaer/invoices
    /// AL forretningslogik for månedlig afregning
    /// </summary>
    public class InvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRackRepository _rackRepository;
        private readonly RentalService _rentalService;
        private readonly SaleService _saleService;

        public InvoiceService(
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            IRackRepository rackRepository,
            RentalService rentalService,
            SaleService saleService)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _rackRepository = rackRepository;
            _rentalService = rentalService;
            _saleService = saleService;
        }

        /// <summary>
        /// Beregner 10% kommission af salget
        /// </summary>
        public decimal CalculateCommission(decimal totalSales)
        {
            return totalSales * 0.10m;
        }

        /// <summary>
        /// Beregner næste måneds reolleje for en kunde
        /// </summary>
        public decimal CalculateNextMonthRent(int customerId)
        {
            var agreements = _rentalService.GetActiveAgreementsForCustomer(customerId);
            return agreements.Sum(a => a.MonthlyRent);
        }

        /// <summary>
        /// Beregner netto beløb: Salg - Kommission - Næste måneds leje
        /// </summary>
        public decimal CalculateNetAmount(decimal totalSales, decimal commission, decimal nextMonthRent)
        {
            return totalSales - commission - nextMonthRent;
        }

        /// <summary>
        /// Opretter en invoice for en specifik kunde
        /// </summary>
        public Invoice CreateInvoiceForCustomer(int customerId, int year, int month)
        {
            var customer = _customerRepository.GetById(customerId);
            if (customer == null)
                return null;

            // Opret basis invoice
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var invoice = new Invoice
            {
                CustomerId = customerId,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                IsCompleted = false,
                IsPaid = false
            };

            // Find kundens reoler
            var customerRacks = _rentalService.GetRacksForCustomer(customerId).ToList();
            if (customerRacks.Count == 0)
                return null;

            invoice.CustomerRacks = customerRacks;

            // Hent salg for perioden
            var rackSales = GetRackSalesForCustomerInPeriod(customerId, startDate, endDate);
            invoice.RackSales = rackSales;

            // Beregn total salg
            invoice.TotalSales = rackSales.Sum(rs => rs.Amount);

            // Beregn kommission
            invoice.CommissionAmount = CalculateCommission(invoice.TotalSales);

            // Beregn næste måneds leje
            invoice.NextMonthRent = CalculateNextMonthRent(customerId);

            // Beregn netto beløb
            invoice.NetAmount = CalculateNetAmount(
                invoice.TotalSales,
                invoice.CommissionAmount,
                invoice.NextMonthRent);

            // Tilføj lejeaftaler
            var rentalAgreements = _rentalService.GetActiveAgreementsForCustomer(customerId).ToList();
            invoice.RentalAgreements = rentalAgreements;

            // Marker som færdig
            invoice.IsCompleted = true;

            // Gem i repository
            invoice = _invoiceRepository.Add(invoice);

            // Fyld navigation property
            invoice.Customer = customer;

            return invoice;
        }

        /// <summary>
        /// Opretter månedlige invoices for alle kunder
        /// </summary>
        public InvoiceGenerationResult CreateMonthlyInvoices(int year, int month)
        {
            var result = new InvoiceGenerationResult();

            // Tjek om der allerede er invoices for perioden
            var existingInvoices = _invoiceRepository.GetByPeriod(year, month).ToList();
            if (existingInvoices.Count > 0)
            {
                result.Success = false;
                result.ErrorMessage = $"Der er allerede oprettet fakturaer for {GetMonthName(month)} {year}";
                return result;
            }

            var createdInvoices = new List<Invoice>();
            var activeCustomers = _customerRepository.GetByStatus(true);

            foreach (var customer in activeCustomers)
            {
                // Tjek om kunden har reoler
                var customerRacks = _rentalService.GetRacksForCustomer(customer.CustomerId);
                if (customerRacks.Any())
                {
                    var invoice = CreateInvoiceForCustomer(customer.CustomerId, year, month);
                    if (invoice != null)
                    {
                        createdInvoices.Add(invoice);
                    }
                }
            }

            result.Success = true;
            result.CreatedInvoices = createdInvoices;
            result.Message = $"Oprettet {createdInvoices.Count} fakturaer for {GetMonthName(month)} {year}";

            return result;
        }

        /// <summary>
        /// Gennemfører udbetaling eller sender regning
        /// </summary>
        public PaymentResult ProcessInvoicePayment(int invoiceId, string paymentMethod)
        {
            var result = new PaymentResult();
            var invoice = _invoiceRepository.GetById(invoiceId);

            if (invoice == null || !invoice.IsCompleted)
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldig eller ikke færdig faktura";
                return result;
            }

            if (invoice.IsPaid)
            {
                result.Success = false;
                result.ErrorMessage = "Fakturaen er allerede betalt/udbetalt";
                return result;
            }

            // Håndter udbetaling (positive beløb)
            if (invoice.NetAmount > 0)
            {
                invoice.IsPaid = true;
                invoice.PaymentDate = DateTime.Now;
                _invoiceRepository.Update(invoice);

                result.Success = true;
                result.Message = $"Udbetaling på {invoice.NetAmount:C0} gennemført";
            }
            // Håndter regning (negative beløb)
            else if (invoice.NetAmount < 0)
            {
                invoice.InvoiceSentDate = DateTime.Now;
                _invoiceRepository.Update(invoice);

                result.Success = true;
                result.Message = $"Regning på {Math.Abs(invoice.NetAmount):C0} sendt";
            }
            // Nul beløb
            else
            {
                invoice.IsPaid = true;
                invoice.PaymentDate = DateTime.Now;
                _invoiceRepository.Update(invoice);

                result.Success = true;
                result.Message = "Lige op - ingen handling nødvendig";
            }

            return result;
        }

        /// <summary>
        /// Henter invoices for en periode
        /// </summary>
        public IEnumerable<Invoice> GetInvoicesForPeriod(int year, int month)
        {
            return _invoiceRepository.GetByPeriod(year, month);
        }

        /// <summary>
        /// Henter invoices der kræver udbetaling
        /// </summary>
        public IEnumerable<Invoice> GetInvoicesForPayout()
        {
            return _invoiceRepository.GetUnpaid()
                .Where(f => f.NetAmount > 0);
        }

        /// <summary>
        /// Henter invoices der kræver regning
        /// </summary>
        public IEnumerable<Invoice> GetInvoicesForBilling()
        {
            return _invoiceRepository.GetUnpaid()
                .Where(f => f.NetAmount < 0);
        }

        /// <summary>
        /// Henter invoices der er lige op
        /// </summary>
        public IEnumerable<Invoice> GetInvoicesWithZeroAmount()
        {
            return _invoiceRepository.GetAll()
                .Where(f => f.IsCompleted && f.NetAmount == 0);
        }

        /// <summary>
        /// Beregner total udestående udbetalinger
        /// </summary>
        public decimal CalculateTotalPendingPayouts()
        {
            return GetInvoicesForPayout().Sum(f => f.NetAmount);
        }

        /// <summary>
        /// Beregner total udestående regninger
        /// </summary>
        public decimal CalculateTotalPendingCharges()
        {
            return GetInvoicesForBilling().Sum(f => Math.Abs(f.NetAmount));
        }

        /// <summary>
        /// Beregner totaler for en periode
        /// </summary>
        public decimal CalculateTotalRevenue(int year, int month)
        {
            var invoices = GetInvoicesForPeriod(year, month);
            return invoices.Sum(f => f.TotalSales);
        }

        public decimal CalculateTotalCommission(int year, int month)
        {
            var invoices = GetInvoicesForPeriod(year, month);
            return invoices.Sum(f => f.CommissionAmount);
        }

        public decimal CalculateTotalRent(int year, int month)
        {
            var invoices = GetInvoicesForPeriod(year, month);
            return invoices.Sum(f => f.NextMonthRent);
        }

        // Private hjælpemetoder

        private List<RackSale> GetRackSalesForCustomerInPeriod(int customerId, DateTime startDate, DateTime endDate)
        {
            var allRackSales = _saleService.GetRackSalesForCustomer(customerId);
            return allRackSales
                .Where(rs => rs.Date.Date >= startDate.Date && rs.Date.Date <= endDate.Date)
                .ToList();
        }

        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };

            return month >= 1 && month <= 12 ? monthNames[month] : "Ukendt";
        }
    }
}