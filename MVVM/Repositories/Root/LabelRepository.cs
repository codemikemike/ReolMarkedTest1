using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly List<Label> _labels;
        private int _nextId;

        public LabelRepository()
        {
            _labels = new List<Label>();
            _nextId = 1;
        }

        public Label Add(Label label)
        {
            label.LabelId = _nextId++;
            label.CreatedAt = DateTime.Now;
            _labels.Add(label);
            return label;
        }

        public Label GetById(int id)
        {
            return _labels.FirstOrDefault(l => l.LabelId == id);
        }

        public IEnumerable<Label> GetAll()
        {
            return _labels.ToList();
        }

        public void Update(Label label)
        {
            var existing = GetById(label.LabelId);
            if (existing != null)
            {
                var index = _labels.IndexOf(existing);
                _labels[index] = label;
            }
        }

        public void Delete(int id)
        {
            var label = GetById(id);
            if (label != null)
            {
                _labels.Remove(label);
            }
        }

        public Label GetByBarcode(string barcode)
        {
            return _labels.FirstOrDefault(l => l.BarCode == barcode);
        }

        public IEnumerable<Label> GetByRackId(int rackId)
        {
            return _labels.Where(l => l.RackId == rackId).ToList();
        }

        public IEnumerable<Label> GetByCustomerId(int customerId)
        {
            return _labels.Where(l => l.Customer != null && l.Customer.CustomerId == customerId).ToList();
        }

        public IEnumerable<Label> GetActiveLabels()
        {
            return _labels.Where(l => !l.IsVoid && !l.SoldDate.HasValue).ToList();
        }

        public IEnumerable<Label> GetSoldLabels()
        {
            return _labels.Where(l => l.SoldDate.HasValue).ToList();
        }
    }
}