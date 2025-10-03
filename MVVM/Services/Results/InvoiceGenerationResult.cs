using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    /// <summary>
    /// Result objekt for invoice generering
    /// </summary>
    public class InvoiceGenerationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<Invoice> CreatedInvoices { get; set; } = new List<Invoice>();
    }
}