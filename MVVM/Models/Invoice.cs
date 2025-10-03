using System;
using System.Collections.Generic;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Ren POCO model for en faktura/invoice
    /// </summary>
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPaid { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        // Amounts
        public decimal TotalSales { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NextMonthRent { get; set; }
        public decimal NetAmount { get; set; }

        // Dates
        public DateTime? PaymentDate { get; set; }
        public DateTime? InvoiceSentDate { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }

        // Collections
        public List<RackSale> RackSales { get; set; } = new List<RackSale>();
        public List<RentalAgreement> RentalAgreements { get; set; } = new List<RentalAgreement>();
        public List<Rack> CustomerRacks { get; set; } = new List<Rack>();
    }
}