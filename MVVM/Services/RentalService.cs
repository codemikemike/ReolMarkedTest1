using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service klasse til at håndtere reol udlejning
    /// Håndterer forretningslogikken for lejeaftaler
    /// </summary>
    public class RentalService
    {
        // Private liste til at gemme alle lejeaftaler
        private List<RentalAgreement> _agreements;
        private int _nextId; // Til at tildele unikke ID'er

        // Reference til repositories
        private CustomerRepository _customerRepo;
        private RackRepository _rackRepo;

        // Konstruktør - opretter service og laver test data
        public RentalService(CustomerRepository customerRepo, RackRepository rackRepo)
        {
            _agreements = new List<RentalAgreement>();
            _nextId = 1;
            _customerRepo = customerRepo;
            _rackRepo = rackRepo;
            CreateTestData();
        }

        /// <summary>
        /// Opretter test data - simulerer Peters eksisterende aftaler
        /// </summary>
        private void CreateTestData()
        {
            // Find Peter Holm (skal oprettes i CustomerRepository først)
            var peter = _customerRepo.AddCustomer("Peter Holm", "12345678", "peter@email.dk", "Nørregade 10, Middelby");

            // Peter har reol 7 og 42 som i use caset
            CreateRentalAgreement(peter, GetRackInfo(7), DateTime.Now.AddMonths(-6));
            CreateRentalAgreement(peter, GetRackInfo(42), DateTime.Now.AddMonths(-3));

            // Lav også en aftale for Mette (reol 15)
            var mette = FindCustomer("23456789"); // Existing customer
            if (mette != null)
            {
                CreateRentalAgreement(mette, GetRackInfo(15), DateTime.Now.AddMonths(-12));
            }
        }

        /// <summary>
        /// Henter rack info baseret på rack nummer
        /// </summary>
        private Rack GetRackInfo(int rackNumber)
        {
            return _rackRepo.GetRackByNumber(rackNumber);
        }

        /// <summary>
        /// Finder kunde baseret på telefonnummer
        /// </summary>
        private Customer FindCustomer(string phoneNumber)
        {
            return _customerRepo.GetCustomerByPhone(phoneNumber);
        }

        /// <summary>
        /// Opretter en ny lejeaftale (hovedmetoden til UC2)
        /// </summary>
        public RentalAgreement CreateRentalAgreement(Customer customer, Rack rack, DateTime startDate)
        {
            // Tjek at kunden og reolen er gyldige
            if (customer == null || rack == null)
                return null;

            // Tjek at reolen er ledig
            if (!rack.IsAvailable)
                return null;

            // Beregn pris baseret på kundens antal reoler
            int customerRackCount = CountRacksForCustomer(customer.CustomerId);

            // Opret ny lejeaftale
            var newAgreement = new RentalAgreement
            {
                AgreementId = _nextId++,
                CustomerId = customer.CustomerId,
                RackId = rack.RackNumber,
                StartDate = startDate,
                Status = "Active",
                CreatedAt = DateTime.Now,
                Notes = $"Lejeaftale oprettet for reol {rack.RackNumber}"
            };

            // Beregn leje baseret på antal reoler (inklusiv den nye)
            newAgreement.CalculateRentDiscount(customerRackCount + 1);

            // Opdater alle kundens eksisterende aftaler med ny pris
            UpdateCustomerRentPrices(customer.CustomerId, customerRackCount + 1);

            // Tilføj til listen
            _agreements.Add(newAgreement);

            // Marker reol som optaget
            _rackRepo.ReserveRack(rack.RackNumber);

            // Fyld navigation properties
            newAgreement.Customer = customer;
            newAgreement.Rack = rack;

            return newAgreement;
        }

        /// <summary>
        /// Henter alle reoler som en kunde lejer (til UC2)
        /// Som når Peter oplyser han har reol 7 og 42
        /// </summary>
        public ObservableCollection<Rack> GetRacksForCustomer(int customerId)
        {
            var customerRacks = new List<Rack>();

            // Gå gennem alle aktive aftaler for kunden
            foreach (var agreement in _agreements)
            {
                if (agreement.CustomerId == customerId && agreement.IsActive)
                {
                    // Find reolen for denne aftale
                    var rack = _rackRepo.GetRackByNumber(agreement.RackId);
                    if (rack != null)
                    {
                        customerRacks.Add(rack);
                    }
                }
            }

            return new ObservableCollection<Rack>(customerRacks);
        }

        /// <summary>
        /// Finder ledige nabo-reoler til alle kundens reoler (til UC2)
        /// </summary>
        public ObservableCollection<Rack> GetAvailableNeighborRacksForCustomer(int customerId)
        {
            var allNeighbors = new List<Rack>();
            var addedRacks = new List<int>(); // For at undgå dubletter

            // Få alle kundens reoler
            var customerRacks = GetRacksForCustomer(customerId);

            // Find nabo-reoler for hver af kundens reoler
            foreach (var rack in customerRacks)
            {
                var neighbors = _rackRepo.GetAvailableNeighborRacks(rack.RackNumber);

                foreach (var neighbor in neighbors)
                {
                    // Undgå dubletter
                    if (!addedRacks.Contains(neighbor.RackNumber))
                    {
                        allNeighbors.Add(neighbor);
                        addedRacks.Add(neighbor.RackNumber);
                    }
                }
            }

            return new ObservableCollection<Rack>(allNeighbors);
        }

        /// <summary>
        /// Henter alle aktive aftaler for en kunde
        /// </summary>
        public ObservableCollection<RentalAgreement> GetActiveAgreementsForCustomer(int customerId)
        {
            var customerAgreements = new List<RentalAgreement>();

            foreach (var agreement in _agreements)
            {
                if (agreement.CustomerId == customerId && agreement.IsActive)
                {
                    // Fyld navigation properties
                    agreement.Customer = _customerRepo.GetCustomerById(agreement.CustomerId);
                    agreement.Rack = _rackRepo.GetRackByNumber(agreement.RackId);
                    customerAgreements.Add(agreement);
                }
            }

            return new ObservableCollection<RentalAgreement>(customerAgreements);
        }

        /// <summary>
        /// Tæller antal reoler en kunde lejer
        /// </summary>
        public int CountRacksForCustomer(int customerId)
        {
            int count = 0;

            foreach (var agreement in _agreements)
            {
                if (agreement.CustomerId == customerId && agreement.IsActive)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Opdaterer leje priser for alle kundens aftaler baseret på samlet antal reoler
        /// </summary>
        private void UpdateCustomerRentPrices(int customerId, int totalRacksCount)
        {
            foreach (var agreement in _agreements)
            {
                if (agreement.CustomerId == customerId && agreement.IsActive)
                {
                    agreement.CalculateRentDiscount(totalRacksCount);
                }
            }
        }

        /// <summary>
        /// Afslutter en lejeaftale
        /// </summary>
        public bool EndAgreement(int agreementId)
        {
            foreach (var agreement in _agreements)
            {
                if (agreement.AgreementId == agreementId && agreement.IsActive)
                {
                    agreement.Status = "Inactive";

                    // Frigør reolen
                    _rackRepo.ReleaseRack(agreement.RackId);

                    // Opdater kundens øvrige aftaler med ny pris
                    int remainingRacks = CountRacksForCustomer(agreement.CustomerId);
                    UpdateCustomerRentPrices(agreement.CustomerId, remainingRacks);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Henter alle aktive aftaler
        /// </summary>
        public ObservableCollection<RentalAgreement> GetAllActiveAgreements()
        {
            var activeAgreements = new List<RentalAgreement>();

            foreach (var agreement in _agreements)
            {
                if (agreement.IsActive)
                {
                    // Fyld navigation properties
                    agreement.Customer = _customerRepo.GetCustomerById(agreement.CustomerId);
                    agreement.Rack = _rackRepo.GetRackByNumber(agreement.RackId);
                    activeAgreements.Add(agreement);
                }
            }

            return new ObservableCollection<RentalAgreement>(activeAgreements);
        }

        /// <summary>
        /// Tæller antal aktive aftaler
        /// </summary>
        public int CountActiveAgreements()
        {
            int count = 0;

            foreach (var agreement in _agreements)
            {
                if (agreement.IsActive)
                {
                    count++;
                }
            }

            return count;
        }
    }
}