using System;

namespace ReolMarked.MVVM.Models
{
    public class Label
    {
        public int LabelId { get; set; }
        public decimal ProductPrice { get; set; }
        public int RackId { get; set; }
        public string BarCode { get; set; } = string.Empty;
        public DateTime? SoldDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVoid { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public Rack Rack { get; set; }
    }
}