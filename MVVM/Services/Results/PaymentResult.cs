using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Sale CompletedSale { get; set; }
        public decimal ChangeGiven { get; set; }
    }
}