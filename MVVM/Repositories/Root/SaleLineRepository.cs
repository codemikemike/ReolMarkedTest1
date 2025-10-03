using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class SaleLineRepository : ISaleLineRepository
    {
        private readonly List<SaleLine> _saleLines;
        private int _nextId;

        public SaleLineRepository()
        {
            _saleLines = new List<SaleLine>();
            _nextId = 1;
        }

        public SaleLine Add(SaleLine saleLine)
        {
            saleLine.SaleLineId = _nextId++;
            _saleLines.Add(saleLine);
            return saleLine;
        }

        public SaleLine GetById(int id)
        {
            return _saleLines.FirstOrDefault(sl => sl.SaleLineId == id);
        }

        public IEnumerable<SaleLine> GetAll()
        {
            return _saleLines.ToList();
        }

        public void Update(SaleLine saleLine)
        {
            var existing = GetById(saleLine.SaleLineId);
            if (existing != null)
            {
                var index = _saleLines.IndexOf(existing);
                _saleLines[index] = saleLine;
            }
        }

        public void Delete(int id)
        {
            var saleLine = GetById(id);
            if (saleLine != null)
            {
                _saleLines.Remove(saleLine);
            }
        }

        public IEnumerable<SaleLine> GetBySaleId(int saleId)
        {
            return _saleLines.Where(sl => sl.SaleId == saleId).ToList();
        }
    }
}