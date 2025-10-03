using System;
using System.Collections.Generic;

namespace ReolMarked.MVVM.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public DateTime SaleDateTime { get; set; }
        public decimal Total { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
        public decimal AmountPaid { get; set; }
        public decimal ChangeGiven { get; set; }
        public bool IsCompleted { get; set; }
        public string Notes { get; set; } = string.Empty;

        public List<SaleLine> SaleLines { get; set; } = new List<SaleLine>();
    }

    public enum PaymentMethod
    {
        Cash,
        Card,
        MobilePay,
        BankTransfer
    }
}