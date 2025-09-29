using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service klasse til at håndtere reol opsigelser (UC5.1)
    /// Håndterer forretningslogikken for opsigelse af reoler som i Louise scenariet
    /// </summary>
    public class TerminationService
    {
        // Private liste til at gemme alle opsigelser
        private List<RackTermination> _terminations;
        private List<RackTermination> _processedTerminations; // NYE - historik over behandlede opsigelser
        private int _nextTerminationId;

        // Reference til andre services og repositories
        private CustomerRepository _customerRepository;
        private RackRepository _rackRepository;
        private RentalService _rentalService;

        // Konstruktør - opretter service
        public TerminationService(CustomerRepository customerRepository, RackRepository rackRepository, RentalService rentalService)
        {
            _terminations = new List<RackTermination>();
            _processedTerminations = new List<RackTermination>(); // NYE - historik
            _nextTerminationId = 1;
            _customerRepository = customerRepository;
            _rackRepository = rackRepository;
            _rentalService = rentalService;
        }

        /// <summary>
        /// Opretter en ny opsigelse (UC5.1 hovedfunktion)
        /// Som når Louise opsiger reol 54
        /// </summary>
        public TerminationResult CreateTermination(int customerId, int rackNumber, DateTime? desiredDate = null, string reason = "")
        {
            var result = new TerminationResult();

            // Valider input
            if (customerId <= 0 || rackNumber <= 0)
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldig kunde eller reolnummer";
                return result;
            }

            // Find kunden
            var customer = _customerRepository.GetCustomerById(customerId);
            if (customer == null)
            {
                result.Success = false;
                result.ErrorMessage = "Kunde ikke fundet";
                return result;
            }

            // Find reolen
            var rack = _rackRepository.GetRackByNumber(rackNumber);
            if (rack == null)
            {
                result.Success = false;
                result.ErrorMessage = "Reol ikke fundet";
                return result;
            }

            // Tjek at kunden har en aktiv aftale for denne reol
            var activeAgreements = _rentalService.GetActiveAgreementsForCustomer(customerId);
            RentalAgreement targetAgreement = null;

            foreach (var agreement in activeAgreements)
            {
                if (agreement.RackId == rackNumber)
                {
                    targetAgreement = agreement;
                    break;
                }
            }

            if (targetAgreement == null)
            {
                result.Success = false;
                result.ErrorMessage = "Kunden har ingen aktiv lejeaftale for denne reol";
                return result;
            }

            // Tjek om der allerede er en opsigelse for denne aftale
            foreach (var existingTermination in _terminations)
            {
                if (existingTermination.AgreementId == targetAgreement.AgreementId && existingTermination.IsActive)
                {
                    result.Success = false;
                    result.ErrorMessage = "Der er allerede en aktiv opsigelse for denne reol";
                    return result;
                }
            }

            // Opret ny opsigelse
            var termination = new RackTermination
            {
                TerminationId = _nextTerminationId++,
                AgreementId = targetAgreement.AgreementId,
                CustomerId = customerId,
                RackNumber = rackNumber,
                RequestDate = DateTime.Now.Date,
                Reason = reason,
                Notes = $"Opsigelse oprettet for {customer.CustomerName}",
                IsProcessed = true // Markeres automatisk som behandlet
            };

            // Sæt navigation properties
            termination.Agreement = targetAgreement;
            termination.Customer = customer;
            termination.Rack = rack;

            // Beregn ikrafttrædelsesdato
            if (desiredDate.HasValue)
            {
                termination.CalculateEffectiveDateForDesiredDate(desiredDate.Value);
            }
            else
            {
                termination.CalculateEffectiveDate();
            }

            // Valider opsigelsen
            if (!termination.IsValid())
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldig opsigelse - tjek datoer og data";
                return result;
            }

            // Tilføj til listen
            _terminations.Add(termination);

            // Returner success
            result.Success = true;
            result.Termination = termination;
            result.Message = $"Opsigelse oprettet for {customer.CustomerName} - Reol {rackNumber}. Træder i kraft {termination.EffectiveDateFormatted}";

            return result;
        }

        /// <summary>
        /// Henter alle opsigelser for en kunde
        /// </summary>
        public ObservableCollection<RackTermination> GetTerminationsForCustomer(int customerId)
        {
            var customerTerminations = new List<RackTermination>();

            foreach (var termination in _terminations)
            {
                if (termination.CustomerId == customerId)
                {
                    customerTerminations.Add(termination);
                }
            }

            return new ObservableCollection<RackTermination>(customerTerminations);
        }

        /// <summary>
        /// Henter alle aktive opsigelser (endnu ikke trådt i kraft)
        /// </summary>
        public ObservableCollection<RackTermination> GetActiveTerminations()
        {
            var activeTerminations = new List<RackTermination>();

            foreach (var termination in _terminations)
            {
                if (termination.IsProcessed && termination.EffectiveDate.Date > DateTime.Now.Date)
                {
                    activeTerminations.Add(termination);
                }
            }

            return new ObservableCollection<RackTermination>(activeTerminations);
        }

        /// <summary>
        /// Henter opsigelser der træder i kraft i dag eller er overskredet
        /// </summary>
        public ObservableCollection<RackTermination> GetTerminationsToProcess()
        {
            var terminationsToProcess = new List<RackTermination>();
            var today = DateTime.Now.Date;

            foreach (var termination in _terminations)
            {
                if (termination.IsProcessed && termination.EffectiveDate.Date <= today)
                {
                    terminationsToProcess.Add(termination);
                }
            }

            return new ObservableCollection<RackTermination>(terminationsToProcess);
        }

        /// <summary>
        /// Henter behandlede opsigelser (historik)
        /// </summary>
        public ObservableCollection<RackTermination> GetProcessedTerminations()
        {
            return new ObservableCollection<RackTermination>(_processedTerminations);
        }

        /// <summary>
        /// Behandler opsigelser der er trådt i kraft
        /// Frigør reoler og afslutter lejeaftaler
        /// </summary>
        public TerminationProcessResult ProcessEffectiveTerminations()
        {
            var result = new TerminationProcessResult();
            var processedTerminations = new List<RackTermination>();

            var terminationsToProcess = GetTerminationsToProcess();

            foreach (var termination in terminationsToProcess)
            {
                // Frigør reolen
                bool rackReleased = _rackRepository.ReleaseRack(termination.RackNumber);

                // Afslut lejeaftalen
                bool agreementEnded = _rentalService.EndAgreement(termination.AgreementId);

                if (rackReleased && agreementEnded)
                {
                    termination.Notes += $" Opsigelse gennemført den {DateTime.Now:dd/MM/yyyy}";

                    // Flyt til historik i stedet for at fjerne helt
                    _processedTerminations.Add(termination);
                    _terminations.Remove(termination);

                    processedTerminations.Add(termination);
                }
            }

            result.Success = true;
            result.ProcessedTerminations = new ObservableCollection<RackTermination>(processedTerminations);
            result.Message = $"Behandlet {processedTerminations.Count} opsigelser";

            return result;
        }

        /// <summary>
        /// Annullerer en opsigelse (hvis kunden fortryder)
        /// </summary>
        public TerminationResult CancelTermination(int terminationId, string reason = "")
        {
            var result = new TerminationResult();

            var termination = GetTerminationById(terminationId);
            if (termination == null)
            {
                result.Success = false;
                result.ErrorMessage = "Opsigelse ikke fundet";
                return result;
            }

            if (!termination.IsActive)
            {
                result.Success = false;
                result.ErrorMessage = "Opsigelsen er allerede trådt i kraft og kan ikke annulleres";
                return result;
            }

            // Fjern fra listen (annuller)
            _terminations.Remove(termination);

            result.Success = true;
            result.Message = $"Opsigelse annulleret for {termination.CustomerName} - {termination.RackNumberDisplay}";
            result.Message += string.IsNullOrEmpty(reason) ? "" : $". Årsag: {reason}";

            return result;
        }

        /// <summary>
        /// Henter alle opsigelser
        /// </summary>
        public ObservableCollection<RackTermination> GetAllTerminations()
        {
            return new ObservableCollection<RackTermination>(_terminations);
        }

        /// <summary>
        /// Finder en opsigelse baseret på ID
        /// </summary>
        public RackTermination GetTerminationById(int terminationId)
        {
            foreach (var termination in _terminations)
            {
                if (termination.TerminationId == terminationId)
                {
                    return termination;
                }
            }

            return null;
        }

        /// <summary>
        /// Tæller aktive opsigelser
        /// </summary>
        public int CountActiveTerminations()
        {
            int count = 0;

            foreach (var termination in _terminations)
            {
                if (termination.IsActive)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Tjekker om en kunde kan opsige en specifik reol
        /// </summary>
        public bool CanCustomerTerminateRack(int customerId, int rackNumber)
        {
            // Tjek at kunden har en aktiv aftale
            var activeAgreements = _rentalService.GetActiveAgreementsForCustomer(customerId);
            bool hasActiveAgreement = false;

            foreach (var agreement in activeAgreements)
            {
                if (agreement.RackId == rackNumber)
                {
                    hasActiveAgreement = true;
                    break;
                }
            }

            if (!hasActiveAgreement)
                return false;

            // Tjek at der ikke allerede er en aktiv opsigelse
            foreach (var termination in _terminations)
            {
                if (termination.CustomerId == customerId &&
                    termination.RackNumber == rackNumber &&
                    termination.IsActive)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Henter månedlig oversigt over opsigelser
        /// </summary>
        public TerminationMonthlyOverview GetMonthlyOverview(int year, int month)
        {
            var overview = new TerminationMonthlyOverview
            {
                Year = year,
                Month = month
            };

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var effectiveThisMonth = new List<RackTermination>();
            var requestedThisMonth = new List<RackTermination>();

            foreach (var termination in _terminations)
            {
                // Opsigelser der træder i kraft denne måned
                if (termination.EffectiveDate.Year == year && termination.EffectiveDate.Month == month)
                {
                    effectiveThisMonth.Add(termination);
                }

                // Opsigelser anmodet denne måned
                if (termination.RequestDate.Year == year && termination.RequestDate.Month == month)
                {
                    requestedThisMonth.Add(termination);
                }
            }

            overview.EffectiveThisMonth = new ObservableCollection<RackTermination>(effectiveThisMonth);
            overview.RequestedThisMonth = new ObservableCollection<RackTermination>(requestedThisMonth);

            return overview;
        }
    }

    /// <summary>
    /// Result klasse for opsigelsesoperationer
    /// </summary>
    public class TerminationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public RackTermination Termination { get; set; }
    }

    /// <summary>
    /// Result klasse for behandling af opsigelser
    /// </summary>
    public class TerminationProcessResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public ObservableCollection<RackTermination> ProcessedTerminations { get; set; } = new();
    }

    /// <summary>
    /// Månedlig oversigt over opsigelser
    /// </summary>
    public class TerminationMonthlyOverview
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public ObservableCollection<RackTermination> EffectiveThisMonth { get; set; } = new();
        public ObservableCollection<RackTermination> RequestedThisMonth { get; set; } = new();

        public string MonthName
        {
            get
            {
                string[] monthNames = {
                    "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                    "Juli", "August", "September", "Oktober", "November", "December"
                };

                return Month >= 1 && Month <= 12 ? monthNames[Month] : "Ukendt";
            }
        }

        public string PeriodDescription => $"{MonthName} {Year}";
    }
}