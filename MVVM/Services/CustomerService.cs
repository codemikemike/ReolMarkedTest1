using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public Customer CreateCustomer(string name, string phone, string email, string address)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Navn må ikke være tomt");

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Telefonnummer må ikke være tomt");

            var existingCustomer = _customerRepository.GetByPhone(phone);
            if (existingCustomer != null)
                throw new InvalidOperationException("Der findes allerede en kunde med dette telefonnummer");

            var customer = new Customer
            {
                Name = name.Trim(),
                Phone = phone.Trim(),
                Email = email?.Trim() ?? string.Empty,
                Address = address?.Trim() ?? string.Empty,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            return _customerRepository.Add(customer);
        }

        public void UpdateCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var existing = _customerRepository.GetById(customer.CustomerId);
            if (existing == null)
                throw new InvalidOperationException("Kunde ikke fundet");

            _customerRepository.Update(customer);
        }

        public void DeactivateCustomer(int customerId)
        {
            var customer = _customerRepository.GetById(customerId);
            if (customer == null)
                throw new InvalidOperationException("Kunde ikke fundet");

            customer.IsActive = false;
            _customerRepository.Update(customer);
        }

        public void ActivateCustomer(int customerId)
        {
            var customer = _customerRepository.GetById(customerId);
            if (customer == null)
                throw new InvalidOperationException("Kunde ikke fundet");

            customer.IsActive = true;
            _customerRepository.Update(customer);
        }

        public Customer FindCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            return _customerRepository.GetByPhone(phone.Trim());
        }

        public IEnumerable<Customer> SearchCustomersByName(string searchText)
        {
            return _customerRepository.SearchByName(searchText);
        }

        public IEnumerable<Customer> GetActiveCustomers()
        {
            return _customerRepository.GetByStatus(true);
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll();
        }

        public int CountActiveCustomers()
        {
            return _customerRepository.GetByStatus(true).Count();
        }

        public Customer GetCustomerById(int customerId)
        {
            return _customerRepository.GetById(customerId);
        }
    }
}