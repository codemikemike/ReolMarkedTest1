using System;
using System.ComponentModel;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Model klasse for opsigelse af reol
    /// Håndterer opsigelser med korrekte frister og datoer som i Louise scenariet
    /// </summary>
    public class RackTermination : INotifyPropertyChanged
    {
        // Private felter til at gemme data
        private int _terminationId;
        private int _agreementId;
        private int _customerId;
        private int _rackNumber;
        private DateTime _requestDate;
        private DateTime _effectiveDate;
        private DateTime _createdAt;
        private string _reason = "";
        private string _notes = "";
        private bool _isProcessed;

        // Navigation properties
        private RentalAgreement _agreement;
        private Customer _customer;
        private Rack _rack;

        // Konstruktør - opretter en ny opsigelse
        public RackTermination()
        {
            // Sæt standard værdier
            RequestDate = DateTime.Now.Date;
            CreatedAt = DateTime.Now;
            IsProcessed = false;
            CalculateEffectiveDate();
        }

        // Properties med NotifyPropertyChanged for databinding

        /// <summary>
        /// Unikt ID for opsigelsen
        /// </summary>
        public int TerminationId
        {
            get { return _terminationId; }
            set
            {
                _terminationId = value;
                OnPropertyChanged(nameof(TerminationId));
            }
        }

        /// <summary>
        /// ID på lejeaftalen der opsiges
        /// </summary>
        public int AgreementId
        {
            get { return _agreementId; }
            set
            {
                _agreementId = value;
                OnPropertyChanged(nameof(AgreementId));
            }
        }

        /// <summary>
        /// ID på kunden der opsiger
        /// </summary>
        public int CustomerId
        {
            get { return _customerId; }
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        /// <summary>
        /// Reolnummer der opsiges
        /// </summary>
        public int RackNumber
        {
            get { return _rackNumber; }
            set
            {
                _rackNumber = value;
                OnPropertyChanged(nameof(RackNumber));
            }
        }

        /// <summary>
        /// Dato for opsigelsesanmodningen
        /// </summary>
        public DateTime RequestDate
        {
            get { return _requestDate; }
            set
            {
                _requestDate = value;
                OnPropertyChanged(nameof(RequestDate));
                OnPropertyChanged(nameof(RequestDateFormatted));
                CalculateEffectiveDate();
            }
        }

        /// <summary>
        /// Dato hvor opsigelsen træder i kraft
        /// </summary>
        public DateTime EffectiveDate
        {
            get { return _effectiveDate; }
            set
            {
                _effectiveDate = value;
                OnPropertyChanged(nameof(EffectiveDate));
                OnPropertyChanged(nameof(EffectiveDateFormatted));
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(DaysUntilEffective));
            }
        }

        /// <summary>
        /// Hvornår opsigelsen blev oprettet
        /// </summary>
        public DateTime CreatedAt
        {
            get { return _createdAt; }
            set
            {
                _createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
                OnPropertyChanged(nameof(CreatedAtFormatted));
            }
        }

        /// <summary>
        /// Årsag til opsigelsen
        /// </summary>
        public string Reason
        {
            get { return _reason; }
            set
            {
                _reason = value;
                OnPropertyChanged(nameof(Reason));
            }
        }

        /// <summary>
        /// Noter til opsigelsen
        /// </summary>
        public string Notes
        {
            get { return _notes; }
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        /// <summary>
        /// Om opsigelsen er behandlet af medarbejder
        /// </summary>
        public bool IsProcessed
        {
            get { return _isProcessed; }
            set
            {
                _isProcessed = value;
                OnPropertyChanged(nameof(IsProcessed));
                OnPropertyChanged(nameof(StatusText));
            }
        }

        // Navigation properties

        /// <summary>
        /// Lejeaftalen der opsiges
        /// </summary>
        public RentalAgreement Agreement
        {
            get { return _agreement; }
            set
            {
                _agreement = value;
                OnPropertyChanged(nameof(Agreement));
            }
        }

        /// <summary>
        /// Kunden der opsiger
        /// </summary>
        public Customer Customer
        {
            get { return _customer; }
            set
            {
                _customer = value;
                OnPropertyChanged(nameof(Customer));
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        /// <summary>
        /// Reolen der opsiges
        /// </summary>
        public Rack Rack
        {
            get { return _rack; }
            set
            {
                _rack = value;
                OnPropertyChanged(nameof(Rack));
                OnPropertyChanged(nameof(RackNumberDisplay));
            }
        }

        // Beregnet properties for UI visning

        /// <summary>
        /// Formateret anmodningsdato
        /// </summary>
        public string RequestDateFormatted
        {
            get { return RequestDate.ToString("dd/MM/yyyy"); }
        }

        /// <summary>
        /// Formateret ikrafttrædelsesdato
        /// </summary>
        public string EffectiveDateFormatted
        {
            get { return EffectiveDate.ToString("dd/MM/yyyy"); }
        }

        /// <summary>
        /// Formateret oprettelsesdato
        /// </summary>
        public string CreatedAtFormatted
        {
            get { return CreatedAt.ToString("dd/MM/yyyy HH:mm"); }
        }

        /// <summary>
        /// Kundens navn
        /// </summary>
        public string CustomerName
        {
            get { return Customer?.CustomerName ?? "Ukendt kunde"; }
        }

        /// <summary>
        /// Reolnummer display
        /// </summary>
        public string RackNumberDisplay
        {
            get { return $"Reol {RackNumber}"; }
        }

        /// <summary>
        /// Om reolen stadig er aktiv (før ikrafttrædelsesdato)
        /// </summary>
        public bool IsActive
        {
            get { return DateTime.Now.Date < EffectiveDate.Date; }
        }

        /// <summary>
        /// Antal dage til opsigelsen træder i kraft
        /// </summary>
        public int DaysUntilEffective
        {
            get
            {
                var days = (EffectiveDate.Date - DateTime.Now.Date).Days;
                return days > 0 ? days : 0;
            }
        }

        /// <summary>
        /// Status tekst for opsigelsen
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!IsProcessed)
                    return "Afventer behandling";
                else if (IsActive)
                    return $"Opsagt til {EffectiveDateFormatted} ({DaysUntilEffective} dage)";
                else
                    return "Opsigelse trådt i kraft";
            }
        }

        /// <summary>
        /// Display tekst til lister
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"{CustomerName} - {RackNumberDisplay} - {StatusText}";
            }
        }

        /// <summary>
        /// Om opsigelsen gælder før eller efter den 20.
        /// </summary>
        public bool IsEarlyTermination
        {
            get { return RequestDate.Day <= 20; }
        }

        /// <summary>
        /// Regel tekst for opsigelsesfristen
        /// </summary>
        public string TerminationRuleText
        {
            get
            {
                if (IsEarlyTermination)
                    return "Opsigelse før den 20. - gælder fra næste måned";
                else
                    return "Opsigelse efter den 20. - gælder fra måneden efter næste måned";
            }
        }

        // Metoder til opsigelseslogik

        /// <summary>
        /// Beregner ikrafttrædelsesdato baseret på opsigelsesfristen
        /// Regel fra Louise scenariet:
        /// - Opsigelse før den 20. gælder fra næste måned
        /// - Opsigelse efter den 20. gælder fra måneden efter næste måned
        /// </summary>
        public void CalculateEffectiveDate()
        {
            DateTime baseDate = RequestDate;

            if (baseDate.Day <= 20)
            {
                // Opsigelse før den 20. - gælder fra næste måned
                EffectiveDate = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(1);
            }
            else
            {
                // Opsigelse efter den 20. - gælder fra måneden efter næste måned
                EffectiveDate = new DateTime(baseDate.Year, baseDate.Month, 1).AddMonths(2);
            }
        }

        /// <summary>
        /// Beregner ikrafttrædelsesdato for en specifik ønsket dato
        /// </summary>
        public void CalculateEffectiveDateForDesiredDate(DateTime desiredDate)
        {
            // Hvis brugeren har valgt en specifik dato, brug den direkte
            EffectiveDate = desiredDate;
        }

        /// <summary>
        /// Validerer at opsigelsen er gyldig
        /// </summary>
        public bool IsValid()
        {
            return AgreementId > 0 &&
                   CustomerId > 0 &&
                   RackNumber > 0;
            // Fjernet dato validering da brugere skal kunne vælge enhver dato
        }

        /// <summary>
        /// Behandler opsigelsen (markerer som behandlet)
        /// </summary>
        public void ProcessTermination()
        {
            if (IsValid())
            {
                IsProcessed = true;
                Notes += $" Behandlet den {DateTime.Now:dd/MM/yyyy HH:mm}";
            }
        }

        /// <summary>
        /// Beregner hvilken måned opsigelsen er gyldig fra
        /// </summary>
        public string GetValidFromMonth()
        {
            return GetMonthName(EffectiveDate.Month) + " " + EffectiveDate.Year;
        }

        /// <summary>
        /// Hjælpemetode til månedsnavne
        /// </summary>
        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };

            return month >= 1 && month <= 12 ? monthNames[month] : "Ukendt";
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sender besked når en property ændres (til databinding)
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}