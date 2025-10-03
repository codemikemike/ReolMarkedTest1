using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers;
        private int _nextId;

        public CustomerRepository()
        {
            _customers = new List<Customer>();
            _nextId = 1;
        }

        public Customer Add(Customer customer)
        {
            customer.CustomerId = _nextId++;
            customer.CreatedAt = DateTime.Now;
            _customers.Add(customer);
            return customer;
        }

        public Customer GetById(int id)
        {
            return _customers.FirstOrDefault(c => c.CustomerId == id);
        }

        public IEnumerable<Customer> GetAll()
        {
            return _customers.ToList();
        }

        public void Update(Customer customer)
        {
            var existing = GetById(customer.CustomerId);
            if (existing != null)
            {
                var index = _customers.IndexOf(existing);
                _customers[index] = customer;
            }
        }

        public void Delete(int id)
        {
            var customer = GetById(id);
            if (customer != null)
            {
                _customers.Remove(customer);
            }
        }

        public Customer GetByPhone(string phone)
        {
            return _customers.FirstOrDefault(c => c.Phone == phone);
        }

        public IEnumerable<Customer> GetByStatus(bool isActive)
        {
            return _customers.Where(c => c.IsActive == isActive).ToList();
        }

        public IEnumerable<Customer> SearchByName(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return Enumerable.Empty<Customer>();

            return _customers
                .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}