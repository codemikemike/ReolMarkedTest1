using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere reol udlejning
    /// Indeholder AL forretningslogik for lejeaftaler
    /// </summary>
    public class RentalService
    {
        private readonly IRentalAgreementRepository _agreementRepository;
        private readonly IRackRepository _rackRepository;
        private readonly ICustomerRepository _customerRepository;

        public RentalService(
            IRentalAgreementRepository agreementRepository,
            IRackRepository rackRepository,
            ICustomerRepository customerRepository)
        {
            _agreementRepository = agreementRepository;
            _rackRepository = rackRepository;
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// Opretter en ny lejeaftale med automatisk prisberegning
        /// </summary>
        public RentalAgreement CreateRentalAgreement(int customerId, int rackId, DateTime startDate)
        {
            var customer = _customerRepository.GetById(customerId);
            var rack = _rackRepository.GetById(rackId);

            if (customer == null)
                throw new InvalidOperationException("Kunde ikke fundet");

            if (rack == null)
                throw new InvalidOperationException("Reol ikke fundet");

            if (!rack.IsAvailable)
                throw new InvalidOperationException("Reol er ikke ledig");

            // Beregn pris baseret på kundens antal reoler
            int currentRackCount = CountActiveRacksForCustomer(customerId);
            decimal price = CalculateRentPrice(currentRackCount + 1);

            // Opret ny aftale
            var agreement = new RentalAgreement
            {
                CustomerId = customerId,
                RackId = rackId,
                StartDate = startDate,
                MonthlyRent = price,
                Status = RentalStatus.Active,
                Notes = $"Lejeaftale oprettet for reol {rack.RackNumber}"
            };

            // Gem i database
            agreement = _agreementRepository.Add(agreement);

            // Opdater rack status
            rack.IsAvailable = false;
            _rackRepository.Update(rack);

            // Opdater alle kundens eksisterende aftaler med ny pris
            UpdateCustomerRentPrices(customerId, currentRackCount + 1);

            // Fyld navigation properties
            agreement.Customer = customer;
            agreement.Rack = rack;

            return agreement;
        }

        /// <summary>
        /// Beregner månedlig leje baseret på antal reoler
        /// 1 reol: 850 kr, 2-3 reoler: 825 kr, 4+ reoler: 800 kr
        /// </summary>
        public decimal CalculateRentPrice(int totalRacksCount)
        {
            if (totalRacksCount == 1)
                return 850m;
            else if (totalRacksCount >= 2 && totalRacksCount <= 3)
                return 825m;
            else if (totalRacksCount >= 4)
                return 800m;

            return 850m; // Default
        }

        /// <summary>
        /// Opdaterer leje priser for alle kundens aftaler
        /// </summary>
        private void UpdateCustomerRentPrices(int customerId, int totalRacksCount)
        {
            var agreements = _agreementRepository.GetByCustomerId(customerId);
            decimal newPrice = CalculateRentPrice(totalRacksCount);

            foreach (var agreement in agreements)
            {
                if (agreement.Status == RentalStatus.Active)
                {
                    agreement.MonthlyRent = newPrice;
                    _agreementRepository.Update(agreement);
                }
            }
        }

        /// <summary>
        /// Henter alle reoler som en kunde lejer
        /// </summary>
        public IEnumerable<Rack> GetRacksForCustomer(int customerId)
        {
            var agreements = _agreementRepository.GetByCustomerId(customerId)
                .Where(a => a.Status == RentalStatus.Active);

            var racks = new List<Rack>();
            foreach (var agreement in agreements)
            {
                var rack = _rackRepository.GetById(agreement.RackId);
                if (rack != null)
                {
                    racks.Add(rack);
                }
            }

            return racks;
        }

        /// <summary>
        /// Finder ledige nabo-reoler til alle kundens reoler
        /// </summary>
        public IEnumerable<Rack> GetAvailableNeighborRacksForCustomer(int customerId)
        {
            var customerRacks = GetRacksForCustomer(customerId);
            var allAvailableRacks = _rackRepository.GetByAvailability(true);
            var neighborRacks = new HashSet<Rack>();

            foreach (var rack in customerRacks)
            {
                // Find nabo-reoler (rackNumber +1 og -1)
                var leftNeighbor = allAvailableRacks.FirstOrDefault(r => r.RackNumber == rack.RackNumber - 1);
                var rightNeighbor = allAvailableRacks.FirstOrDefault(r => r.RackNumber == rack.RackNumber + 1);

                if (leftNeighbor != null)
                    neighborRacks.Add(leftNeighbor);

                if (rightNeighbor != null)
                    neighborRacks.Add(rightNeighbor);
            }

            return neighborRacks;
        }

        /// <summary>
        /// Henter alle aktive aftaler for en kunde
        /// </summary>
        public IEnumerable<RentalAgreement> GetActiveAgreementsForCustomer(int customerId)
        {
            var agreements = _agreementRepository.GetByCustomerId(customerId)
                .Where(a => a.Status == RentalStatus.Active);

            // Fyld navigation properties
            foreach (var agreement in agreements)
            {
                agreement.Customer = _customerRepository.GetById(agreement.CustomerId);
                agreement.Rack = _rackRepository.GetById(agreement.RackId);
            }

            return agreements;
        }

        /// <summary>
        /// Tæller antal aktive reoler for en kunde
        /// </summary>
        public int CountActiveRacksForCustomer(int customerId)
        {
            return _agreementRepository.GetByCustomerId(customerId)
                .Count(a => a.Status == RentalStatus.Active);
        }

        /// <summary>
        /// Afslutter en lejeaftale
        /// </summary>
        public bool EndAgreement(int agreementId)
        {
            var agreement = _agreementRepository.GetById(agreementId);
            if (agreement == null || agreement.Status != RentalStatus.Active)
                return false;

            // Sæt status til terminated
            agreement.Status = RentalStatus.Terminated;
            _agreementRepository.Update(agreement);

            // Frigør reolen
            var rack = _rackRepository.GetById(agreement.RackId);
            if (rack != null)
            {
                rack.IsAvailable = true;
                _rackRepository.Update(rack);
            }

            // Opdater kundens øvrige aftaler med ny pris
            int remainingRacks = CountActiveRacksForCustomer(agreement.CustomerId);
            UpdateCustomerRentPrices(agreement.CustomerId, remainingRacks);

            return true;
        }

        /// <summary>
        /// Henter alle aktive aftaler
        /// </summary>
        public IEnumerable<RentalAgreement> GetAllActiveAgreements()
        {
            var agreements = _agreementRepository.GetByStatus(RentalStatus.Active);

            // Fyld navigation properties
            foreach (var agreement in agreements)
            {
                agreement.Customer = _customerRepository.GetById(agreement.CustomerId);
                agreement.Rack = _rackRepository.GetById(agreement.RackId);
            }

            return agreements;
        }
    }
}