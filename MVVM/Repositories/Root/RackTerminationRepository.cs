// Repositories/RackTerminationRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackTerminationRepository : IRackTerminationRepository
    {
        private readonly List<RackTermination> _terminations;
        private int _nextId;

        public RackTerminationRepository()
        {
            _terminations = new List<RackTermination>();
            _nextId = 1;
        }

        public RackTermination Add(RackTermination termination)
        {
            termination.TerminationId = _nextId++;
            termination.CreatedAt = DateTime.Now;
            _terminations.Add(termination);
            return termination;
        }

        public RackTermination GetById(int id)
        {
            return _terminations.FirstOrDefault(t => t.TerminationId == id);
        }

        public IEnumerable<RackTermination> GetAll()
        {
            return _terminations.ToList();
        }

        public void Update(RackTermination termination)
        {
            var existing = GetById(termination.TerminationId);
            if (existing != null)
            {
                var index = _terminations.IndexOf(existing);
                _terminations[index] = termination;
            }
        }

        public void Delete(int id)
        {
            var termination = GetById(id);
            if (termination != null)
            {
                _terminations.Remove(termination);
            }
        }

        public IEnumerable<RackTermination> GetByCustomerId(int customerId)
        {
            return _terminations.Where(t => t.CustomerId == customerId).ToList();
        }

        public IEnumerable<RackTermination> GetByAgreementId(int agreementId)
        {
            return _terminations.Where(t => t.AgreementId == agreementId).ToList();
        }

        public IEnumerable<RackTermination> GetActive()
        {
            return _terminations.Where(t => t.EffectiveDate > DateTime.Now).ToList();
        }

        public IEnumerable<RackTermination> GetEffectiveBeforeDate(DateTime date)
        {
            return _terminations.Where(t => t.EffectiveDate.Date <= date.Date && t.IsProcessed).ToList();
        }
    }
}