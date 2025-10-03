// Services/Results/TerminationResult.cs

using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    /// <summary>
    /// Result objekt for opsigelsesoperationer
    /// </summary>
    public class TerminationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public RackTermination Termination { get; set; }
    }
}