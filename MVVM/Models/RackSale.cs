using System;

namespace ReolMarked.MVVM.Models
{
    public class RackSale
    {
        public int RackSaleId { get; set; }
        public int SaleId { get; set; }
        public int RackNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string ProductInfo { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public Sale Sale { get; set; }
        public Rack Rack { get; set; }
        public Customer Customer { get; set; }
    }
}