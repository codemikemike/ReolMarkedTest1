using System;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Ren POCO model for en reol kontrakt
    /// </summary>
    public class RackContract
    {
        public int ContractId { get; set; }
        public int CustomerId { get; set; }
        public int RackId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; } = "";

        // Navigation properties
        public Customer Customer { get; set; }
        public Rack Rack { get; set; }
    }
}