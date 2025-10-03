// Services/RackService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere reol-relateret forretningslogik
    /// </summary>
    public class RackService
    {
        private readonly IRackRepository _rackRepository;

        public RackService(IRackRepository rackRepository)
        {
            _rackRepository = rackRepository;
        }

        /// <summary>
        /// Henter alle ledige reoler
        /// </summary>
        public IEnumerable<Rack> GetAvailableRacks()
        {
            return _rackRepository.GetByAvailability(true);
        }

        /// <summary>
        /// Henter ledige reoler uden bøjlestang
        /// </summary>
        public IEnumerable<Rack> GetAvailableRacksWithoutHangerBar()
        {
            return _rackRepository.GetByAvailability(true)
                .Where(r => !r.HasHangerBar);
        }

        /// <summary>
        /// Henter optagne reoler
        /// </summary>
        public IEnumerable<Rack> GetOccupiedRacks()
        {
            return _rackRepository.GetByAvailability(false);
        }

        /// <summary>
        /// Finder ledige nabo-reoler til en given reol
        /// </summary>
        public IEnumerable<Rack> GetAvailableNeighborRacks(int rackNumber)
        {
            var availableRacks = GetAvailableRacks().ToList();
            var neighbors = new List<Rack>();

            // Venstre nabo
            var leftNeighbor = availableRacks.FirstOrDefault(r => r.RackNumber == rackNumber - 1);
            if (leftNeighbor != null)
                neighbors.Add(leftNeighbor);

            // Højre nabo
            var rightNeighbor = availableRacks.FirstOrDefault(r => r.RackNumber == rackNumber + 1);
            if (rightNeighbor != null)
                neighbors.Add(rightNeighbor);

            return neighbors;
        }

        /// <summary>
        /// Reserverer en reol (sætter IsAvailable = false)
        /// </summary>
        public void ReserveRack(int rackId)
        {
            var rack = _rackRepository.GetById(rackId);
            if (rack == null)
                throw new InvalidOperationException("Reol ikke fundet");

            if (!rack.IsAvailable)
                throw new InvalidOperationException("Reol er allerede optaget");

            rack.IsAvailable = false;
            _rackRepository.Update(rack);
        }

        /// <summary>
        /// Frigør en reol (sætter IsAvailable = true)
        /// </summary>
        public void ReleaseRack(int rackId)
        {
            var rack = _rackRepository.GetById(rackId);
            if (rack == null)
                throw new InvalidOperationException("Reol ikke fundet");

            rack.IsAvailable = true;
            _rackRepository.Update(rack);
        }

        /// <summary>
        /// Tæller antal ledige reoler
        /// </summary>
        public int CountAvailableRacks()
        {
            return _rackRepository.GetByAvailability(true).Count();
        }

        /// <summary>
        /// Tæller antal optagne reoler
        /// </summary>
        public int CountOccupiedRacks()
        {
            return _rackRepository.GetByAvailability(false).Count();
        }

        /// <summary>
        /// Henter en reol baseret på reolnummer
        /// </summary>
        public Rack GetRackByNumber(int rackNumber)
        {
            return _rackRepository.GetByRackNumber(rackNumber);
        }

        /// <summary>
        /// Henter alle reoler
        /// </summary>
        public IEnumerable<Rack> GetAllRacks()
        {
            return _rackRepository.GetAll();
        }
    }
}