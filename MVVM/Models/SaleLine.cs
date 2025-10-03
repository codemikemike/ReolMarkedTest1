using System;

namespace ReolMarked.MVVM.Models
{
    public class SaleLine
    {
        public int SaleLineId { get; set; }
        public int SaleId { get; set; }
        public int LabelId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation properties
        public Sale Sale { get; set; }
        public Label Label { get; set; }
    }
}