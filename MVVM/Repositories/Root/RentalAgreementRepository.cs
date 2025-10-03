using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RentalAgreementRepository : IRentalAgreementRepository
    {
        private readonly List<RentalAgreement> _agreements;
        private int _nextId;

        public RentalAgreementRepository()
        {
            _agreements = new List<RentalAgreement>();
            _nextId = 1;
        }

        public RentalAgreement Add(RentalAgreement agreement)
        {
            agreement.AgreementId = _nextId++;
            agreement.CreatedAt = DateTime.Now;
            _agreements.Add(agreement);
            return agreement;
        }

        public RentalAgreement GetById(int id)
        {
            return _agreements.FirstOrDefault(a => a.AgreementId == id);
        }

        public IEnumerable<RentalAgreement> GetAll()
        {
            return _agreements.ToList();
        }

        public void Update(RentalAgreement agreement)
        {
            var existing = GetById(agreement.AgreementId);
            if (existing != null)
            {
                var index = _agreements.IndexOf(existing);
                _agreements[index] = agreement;
            }
        }

        public void Delete(int id)
        {
            var agreement = GetById(id);
            if (agreement != null)
            {
                _agreements.Remove(agreement);
            }
        }

        public IEnumerable<RentalAgreement> GetByCustomerId(int customerId)
        {
            return _agreements.Where(a => a.CustomerId == customerId).ToList();
        }

        public IEnumerable<RentalAgreement> GetByRackId(int rackId)
        {
            return _agreements.Where(a => a.RackId == rackId).ToList();
        }

        public IEnumerable<RentalAgreement> GetByStatus(RentalStatus status)
        {
            return _agreements.Where(a => a.Status == status).ToList();
        }

        public RentalAgreement GetActiveAgreementForRack(int rackId)
        {
            return _agreements.FirstOrDefault(a => a.RackId == rackId && a.Status == RentalStatus.Active);
        }
    }
}