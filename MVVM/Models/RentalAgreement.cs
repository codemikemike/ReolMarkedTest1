using System;

namespace ReolMarked.MVVM.Models
{
    public class RentalAgreement
    {
        public int AgreementId { get; set; }
        public int CustomerId { get; set; }
        public int RackId { get; set; }
        public DateTime StartDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Active;
        public DateTime CreatedAt { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public Customer Customer { get; set; }
        public Rack Rack { get; set; }
    }

    public enum RentalStatus
    {
        Active,
        Terminated,
        Pending,
        Expired
    }
}