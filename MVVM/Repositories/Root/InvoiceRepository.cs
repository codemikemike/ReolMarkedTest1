using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly List<Invoice> _invoices;
        private int _nextId;

        public InvoiceRepository()
        {
            _invoices = new List<Invoice>();
            _nextId = 1;
        }

        public Invoice Add(Invoice invoice)
        {
            invoice.InvoiceId = _nextId++;
            invoice.CreatedAt = DateTime.Now;
            _invoices.Add(invoice);
            return invoice;
        }

        public Invoice GetById(int id)
        {
            return _invoices.FirstOrDefault(f => f.InvoiceId == id);
        }

        public IEnumerable<Invoice> GetAll()
        {
            return _invoices.ToList();
        }

        public void Update(Invoice invoice)
        {
            var existing = GetById(invoice.InvoiceId);
            if (existing != null)
            {
                var index = _invoices.IndexOf(existing);
                _invoices[index] = invoice;
            }
        }

        public void Delete(int id)
        {
            var invoice = GetById(id);
            if (invoice != null)
            {
                _invoices.Remove(invoice);
            }
        }

        public IEnumerable<Invoice> GetByCustomerId(int customerId)
        {
            return _invoices.Where(f => f.CustomerId == customerId).ToList();
        }

        public IEnumerable<Invoice> GetByPeriod(int year, int month)
        {
            return _invoices.Where(f => f.PeriodStart.Year == year && f.PeriodStart.Month == month).ToList();
        }

        public IEnumerable<Invoice> GetUnpaid()
        {
            return _invoices.Where(f => !f.IsPaid && f.IsCompleted).ToList();
        }
    }
}