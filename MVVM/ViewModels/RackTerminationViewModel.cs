using System;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.ViewModels.Base;

namespace ReolMarked.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel der wrapper RackTermination model
    /// </summary>
    public class RackTerminationViewModel : ViewModelBase
    {
        private readonly RackTermination _model;

        public RackTerminationViewModel(RackTermination model)
        {
            _model = model;
        }

        public int TerminationId => _model.TerminationId;
        public int RackNumber => _model.RackNumber;
        public DateTime RequestDate => _model.RequestDate;
        public DateTime EffectiveDate => _model.EffectiveDate;
        public string Reason => _model.Reason;
        public bool IsProcessed => _model.IsProcessed;

        // UI-specific properties
        public string CustomerName => _model.Customer?.Name ?? "Ukendt";
        public string RequestDateFormatted => RequestDate.ToString("dd/MM/yyyy");
        public string EffectiveDateFormatted => EffectiveDate.ToString("dd/MM/yyyy");
        public bool IsActive => EffectiveDate > DateTime.Now;
        public int DaysUntilEffective => Math.Max(0, (EffectiveDate.Date - DateTime.Now.Date).Days);

        public string StatusText
        {
            get
            {
                if (!IsProcessed) return "Afventer behandling";
                if (IsActive) return $"Opsagt til {EffectiveDateFormatted} ({DaysUntilEffective} dage)";
                return "Opsigelse trådt i kraft";
            }
        }

        public string DisplayText => $"{CustomerName} - Reol {RackNumber} - {StatusText}";

        public RackTermination Model => _model;
    }
}