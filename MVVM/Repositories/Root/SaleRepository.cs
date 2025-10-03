using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class SaleRepository : ISaleRepository
    {
        private readonly List<Sale> _sales;
        private int _nextId;

        public SaleRepository()
        {
            _sales = new List<Sale>();
            _nextId = 1;
        }

        public Sale Add(Sale sale)
        {
            sale.SaleId = _nextId++;
            _sales.Add(sale);
            return sale;
        }

        public Sale GetById(int id)
        {
            return _sales.FirstOrDefault(s => s.SaleId == id);
        }

        public IEnumerable<Sale> GetAll()
        {
            return _sales.ToList();
        }

        public void Update(Sale sale)
        {
            var existing = GetById(sale.SaleId);
            if (existing != null)
            {
                var index = _sales.IndexOf(existing);
                _sales[index] = sale;
            }
        }

        public void Delete(int id)
        {
            var sale = GetById(id);
            if (sale != null)
            {
                _sales.Remove(sale);
            }
        }

        public IEnumerable<Sale> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            return _sales.Where(s => s.SaleDateTime.Date >= fromDate.Date && s.SaleDateTime.Date <= toDate.Date).ToList();
        }

        public IEnumerable<Sale> GetCompletedSales()
        {
            return _sales.Where(s => s.IsCompleted).ToList();
        }
    }
}