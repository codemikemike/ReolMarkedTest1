// Services/Results/TerminationProcessResult.cs

using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    /// <summary>
    /// Result objekt for behandling af opsigelser
    /// </summary>
    public class TerminationProcessResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public List<RackTermination> ProcessedTerminations { get; set; } = new();
    }
}