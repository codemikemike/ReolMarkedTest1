// Services/Results/ScanResult.cs

using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    /// <summary>
    /// Result objekt for scanner operationer
    /// </summary>
    public class ScanResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public SaleLine AddedSaleLine { get; set; }
    }
}