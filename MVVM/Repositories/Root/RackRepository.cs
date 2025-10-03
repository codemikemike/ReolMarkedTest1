// Repositories/RackRepository.cs
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackRepository : IRackRepository
    {
        private readonly List<Rack> _racks;

        public RackRepository()
        {
            _racks = new List<Rack>();
        }

        public Rack Add(Rack rack)
        {
            _racks.Add(rack);
            return rack;
        }

        public Rack GetById(int id)
        {
            return _racks.FirstOrDefault(r => r.RackId == id);
        }

        public IEnumerable<Rack> GetAll()
        {
            return _racks.ToList();
        }

        public void Update(Rack rack)
        {
            var existing = GetById(rack.RackId);
            if (existing != null)
            {
                var index = _racks.IndexOf(existing);
                _racks[index] = rack;
            }
        }

        public void Delete(int id)
        {
            var rack = GetById(id);
            if (rack != null)
            {
                _racks.Remove(rack);
            }
        }

        public Rack GetByRackNumber(int rackNumber)
        {
            return _racks.FirstOrDefault(r => r.RackNumber == rackNumber);
        }

        public IEnumerable<Rack> GetByAvailability(bool isAvailable)
        {
            return _racks.Where(r => r.IsAvailable == isAvailable).ToList();
        }

        public IEnumerable<Rack> GetByHangerBarStatus(bool hasHangerBar)
        {
            return _racks.Where(r => r.HasHangerBar == hasHangerBar).ToList();
        }
    }
}