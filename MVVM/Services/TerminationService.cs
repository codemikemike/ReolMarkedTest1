using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services.Results;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere reol opsigelser
    /// AL forretningslogik for opsigelser
    /// </summary>
    public class TerminationService
    {
        private readonly IRackTerminationRepository _terminationRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRackRepository _rackRepository;
        private readonly RentalService _rentalService;

        public TerminationService(
            IRackTerminationRepository terminationRepository,
            ICustomerRepository customerRepository,
            IRackRepository rackRepository,
            RentalService rentalService)
        {
            _terminationRepository = terminationRepository;
            _customerRepository = customerRepository;
            _rackRepository = rackRepository;
            _rentalService = rentalService;
        }

        /// <summary>
        /// Beregner ikrafttrædelsesdato baseret på opsigelsesfristen
        /// Regel: Før den 20. = næste måned, efter den 20. = måneden efter
        /// </summary>
        public DateTime CalculateEffectiveDate(DateTime requestDate)
        {
            if (requestDate.Day <= 20)
            {
                // Opsigelse før den 20. - gælder fra næste måned
                return new DateTime(requestDate.Year, requestDate.Month, 1).AddMonths(1);
            }
            else
            {
                // Opsigelse efter den 20. - gælder fra måneden efter næste måned
                return new DateTime(requestDate.Year, requestDate.Month, 1).AddMonths(2);
            }
        }

        /// <summary>
        /// Opretter en ny opsigelse
        /// </summary>
        public TerminationResult CreateTermination(
            int customerId,
            int rackNumber,
            DateTime? desiredDate = null,
            string reason = "")
        {
            var result = new TerminationResult();

            var customer = _customerRepository.GetById(customerId);
            var rack = _rackRepository.GetByRackNumber(rackNumber);

            if (customer == null)
            {
                result.Success = false;
                result.ErrorMessage = "Kunde ikke fundet";
                return result;
            }

            if (rack == null)
            {
                result.Success = false;
                result.ErrorMessage = "Reol ikke fundet";
                return result;
            }

            // Find aktiv aftale
            var agreements = _rentalService.GetActiveAgreementsForCustomer(customerId);
            var targetAgreement = agreements.FirstOrDefault(a => a.RackId == rack.RackId);

            if (targetAgreement == null)
            {
                result.Success = false;
                result.ErrorMessage = "Kunden har ingen aktiv lejeaftale for denne reol";
                return result;
            }

            // Tjek for eksisterende opsigelse
            var existingTerminations = _terminationRepository.GetByAgreementId(targetAgreement.AgreementId);
            if (existingTerminations.Any(t => t.EffectiveDate > DateTime.Now))
            {
                result.Success = false;
                result.ErrorMessage = "Der er allerede en aktiv opsigelse for denne reol";
                return result;
            }

            // Opret opsigelse
            var termination = new RackTermination
            {
                AgreementId = targetAgreement.AgreementId,
                CustomerId = customerId,
                RackNumber = rackNumber,
                RequestDate = DateTime.Now.Date,
                Reason = reason,
                Notes = $"Opsigelse oprettet for {customer.Name}",
                IsProcessed = true
            };

            // Beregn ikrafttrædelsesdato
            termination.EffectiveDate = desiredDate ?? CalculateEffectiveDate(termination.RequestDate);

            // Gem i repository
            termination = _terminationRepository.Add(termination);

            // Fyld navigation properties
            termination.Agreement = targetAgreement;
            termination.Customer = customer;
            termination.Rack = rack;

            result.Success = true;
            result.Termination = termination;
            result.Message = $"Opsigelse oprettet - træder i kraft {termination.EffectiveDate:dd/MM/yyyy}";

            return result;
        }

        /// <summary>
        /// Behandler opsigelser der er trådt i kraft
        /// </summary>
        public TerminationProcessResult ProcessEffectiveTerminations()
        {
            var result = new TerminationProcessResult();
            var terminationsToProcess = _terminationRepository
                .GetEffectiveBeforeDate(DateTime.Now.Date)
                .ToList();

            var processedTerminations = new List<RackTermination>();

            foreach (var termination in terminationsToProcess)
            {
                // Afslut lejeaftale
                bool agreementEnded = _rentalService.EndAgreement(termination.AgreementId);

                if (agreementEnded)
                {
                    termination.Notes += $" Behandlet {DateTime.Now:dd/MM/yyyy}";
                    _terminationRepository.Update(termination);
                    processedTerminations.Add(termination);
                }
            }

            result.Success = true;
            result.ProcessedTerminations = processedTerminations;
            result.Message = $"Behandlet {processedTerminations.Count} opsigelser";

            return result;
        }

        /// <summary>
        /// Henter aktive opsigelser
        /// </summary>
        public IEnumerable<RackTermination> GetActiveTerminations()
        {
            return _terminationRepository.GetActive();
        }

        /// <summary>
        /// Henter opsigelser for en kunde
        /// </summary>
        public IEnumerable<RackTermination> GetTerminationsForCustomer(int customerId)
        {
            return _terminationRepository.GetByCustomerId(customerId);
        }

        /// <summary>
        /// Annullerer en opsigelse
        /// </summary>
        public TerminationResult CancelTermination(int terminationId, string reason = "")
        {
            var result = new TerminationResult();
            var termination = _terminationRepository.GetById(terminationId);

            if (termination == null)
            {
                result.Success = false;
                result.ErrorMessage = "Opsigelse ikke fundet";
                return result;
            }

            if (termination.EffectiveDate <= DateTime.Now)
            {
                result.Success = false;
                result.ErrorMessage = "Opsigelsen er allerede trådt i kraft";
                return result;
            }

            _terminationRepository.Delete(terminationId);

            result.Success = true;
            result.Message = $"Opsigelse annulleret. {reason}";

            return result;
        }
    }
}