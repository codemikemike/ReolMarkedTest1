using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackSaleRepository : IRackSaleRepository
    {
        private readonly List<RackSale> _rackSales;
        private int _nextId;

        public RackSaleRepository()
        {
            _rackSales = new List<RackSale>();
            _nextId = 1;
        }

        public RackSale Add(RackSale rackSale)
        {
            rackSale.RackSaleId = _nextId++;
            _rackSales.Add(rackSale);
            return rackSale;
        }

        public RackSale GetById(int id)
        {
            return _rackSales.FirstOrDefault(rs => rs.RackSaleId == id);
        }

        public IEnumerable<RackSale> GetAll()
        {
            return _rackSales.ToList();
        }

        public void Update(RackSale rackSale)
        {
            var existing = GetById(rackSale.RackSaleId);
            if (existing != null)
            {
                var index = _rackSales.IndexOf(existing);
                _rackSales[index] = rackSale;
            }
        }

        public void Delete(int id)
        {
            var rackSale = GetById(id);
            if (rackSale != null)
            {
                _rackSales.Remove(rackSale);
            }
        }

        public IEnumerable<RackSale> GetByCustomerId(int customerId)
        {
            return _rackSales.Where(rs => rs.CustomerId == customerId).ToList();
        }

        public IEnumerable<RackSale> GetByRackNumber(int rackNumber)
        {
            return _rackSales.Where(rs => rs.RackNumber == rackNumber).ToList();
        }

        public IEnumerable<RackSale> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            return _rackSales.Where(rs => rs.Date.Date >= fromDate.Date && rs.Date.Date <= toDate.Date).ToList();
        }
    }
}