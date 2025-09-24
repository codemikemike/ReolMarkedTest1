using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories
{
    /// <summary>
    /// Repository klasse til at håndtere reol data
    /// Erstatter kartotekskort systemet fra use case
    /// </summary>
    public class RackRepository
    {
        // Private liste til at gemme alle reoler
        private List<Rack> _racks;

        // Konstruktør - opretter repository og laver test data
        public RackRepository()
        {
            _racks = new List<Rack>();
            CreateTestData();
        }

        /// <summary>
        /// Opretter test data - simulerer de 80 reoler i Middelby Reolmarked
        /// </summary>
        private void CreateTestData()
        {
            // Lav 80 reoler som i use caset
            for (int i = 1; i <= 80; i++)
            {
                var rack = new Rack
                {
                    RackId = i,
                    RackNumber = i,
                    AmountShelves = 6, // Som standard 6 hylder
                    HasHangerBar = (i % 10 == 0), // Hver 10. reol har bøjlestang
                    Location = GetLocationForRack(i),
                    IsAvailable = (i <= 60), // De første 60 er ledige, resten optaget
                    Description = $"Reol nummer {i}"
                };

                _racks.Add(rack);
            }
        }

        /// <summary>
        /// Giver en placering baseret på reol nummer
        /// </summary>
        private string GetLocationForRack(int rackNumber)
        {
            if (rackNumber <= 20)
                return "Indgang højre";
            else if (rackNumber <= 40)
                return "Midtergang venstre";
            else if (rackNumber <= 60)
                return "Midtergang højre";
            else
                return "Bagved - roligt område";
        }

        /// <summary>
        /// Henter alle reoler
        /// </summary>
        public ObservableCollection<Rack> GetAllRacks()
        {
            return new ObservableCollection<Rack>(_racks);
        }

        /// <summary>
        /// Henter kun ledige reoler (som Mettes kasse med ledige kort)
        /// </summary>
        public ObservableCollection<Rack> GetAvailableRacks()
        {
            var availableRacks = new List<Rack>();

            // Gå gennem alle reoler og find de ledige
            foreach (var rack in _racks)
            {
                if (rack.IsAvailable)
                {
                    availableRacks.Add(rack);
                }
            }

            return new ObservableCollection<Rack>(availableRacks);
        }

        /// <summary>
        /// Henter ledige reoler med kun hylder (som Anton ønskede)
        /// </summary>
        public ObservableCollection<Rack> GetAvailableRacksWithoutHangerBar()
        {
            var filteredRacks = new List<Rack>();

            // Gå gennem alle reoler og find de ledige uden bøjlestang
            foreach (var rack in _racks)
            {
                if (rack.IsAvailable && !rack.HasHangerBar)
                {
                    filteredRacks.Add(rack);
                }
            }

            return new ObservableCollection<Rack>(filteredRacks);
        }

        /// <summary>
        /// Henter optagne reoler (som Mettes kasse med optagne kort)
        /// </summary>
        public ObservableCollection<Rack> GetOccupiedRacks()
        {
            var occupiedRacks = new List<Rack>();

            // Gå gennem alle reoler og find de optagne
            foreach (var rack in _racks)
            {
                if (!rack.IsAvailable)
                {
                    occupiedRacks.Add(rack);
                }
            }

            return new ObservableCollection<Rack>(occupiedRacks);
        }

        /// <summary>
        /// Finder en specifik reol baseret på reol nummer
        /// </summary>
        public Rack GetRackByNumber(int rackNumber)
        {
            // Gå gennem alle reoler og find den med det rigtige nummer
            foreach (var rack in _racks)
            {
                if (rack.RackNumber == rackNumber)
                {
                    return rack;
                }
            }

            // Returner null hvis reolen ikke findes
            return null;
        }

        /// <summary>
        /// Markerer en reol som optaget (når en kunde lejer den)
        /// </summary>
        public bool ReserveRack(int rackNumber)
        {
            var rack = GetRackByNumber(rackNumber);

            if (rack != null && rack.IsAvailable)
            {
                rack.IsAvailable = false;
                return true; // Reolen blev reserveret
            }

            return false; // Reolen kunne ikke reserveres
        }

        /// <summary>
        /// Markerer en reol som ledig igen (når kontrakt ophører)
        /// </summary>
        public bool ReleaseRack(int rackNumber)
        {
            var rack = GetRackByNumber(rackNumber);

            if (rack != null && !rack.IsAvailable)
            {
                rack.IsAvailable = true;
                return true; // Reolen blev frigivet
            }

            return false; // Reolen kunne ikke frigives
        }

        /// <summary>
        /// Tæller antal ledige reoler
        /// </summary>
        public int CountAvailableRacks()
        {
            int count = 0;

            foreach (var rack in _racks)
            {
                if (rack.IsAvailable)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Tæller antal optagne reoler
        /// </summary>
        public int CountOccupiedRacks()
        {
            int count = 0;

            foreach (var rack in _racks)
            {
                if (!rack.IsAvailable)
                {
                    count++;
                }
            }

            return count;
        }
    }
}