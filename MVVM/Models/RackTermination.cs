using System;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Ren POCO model for en opsigelse
    /// </summary>
    public class RackTermination
    {
        public int TerminationId { get; set; }
        public int AgreementId { get; set; }
        public int CustomerId { get; set; }
        public int RackNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Reason { get; set; } = "";
        public string Notes { get; set; } = "";
        public bool IsProcessed { get; set; }

        // Navigation properties
        public RentalAgreement Agreement { get; set; }
        public Customer Customer { get; set; }
        public Rack Rack { get; set; }
    }
}