// Flyt til: Services/Results/BarcodeGenerationResult.cs

using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services.Results
{
    /// <summary>
    /// Result objekt for barcode generering operationer
    /// Returneres fra BarcodeService
    /// </summary>
    public class BarcodeGenerationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public List<Label> CreatedLabels { get; set; } = new();
        public string PrintOutput { get; set; } = "";

        // Computed properties er OK her da det er et result object
        public int LabelCount => CreatedLabels?.Count ?? 0;
        public bool HasError => !Success && !string.IsNullOrEmpty(ErrorMessage);
        public bool HasLabels => LabelCount > 0;
    }
}