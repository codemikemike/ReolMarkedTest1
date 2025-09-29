using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories
{
    /// <summary>
    /// Repository klasse til at håndtere kunde data
    /// Gemmer og henter kunde oplysninger
    /// </summary>
    public class CustomerRepository
    {
        // Private liste til at gemme alle kunder
        private List<Customer> _customers;
        private int _nextId; // Til at tildele unikke ID'er

        // Konstruktør - opretter repository og laver test data
        public CustomerRepository()
        {
            _customers = new List<Customer>();
            _nextId = 1;
            CreateTestData();
        }

        /// <summary>
        /// Opretter nogle test kunder
        /// </summary>
        private void CreateTestData()
        {
            // Tilføj test-kunde Peter (bruges i BarcodeService test-data)
            AddCustomer("Peter Hansen", "12345678", "peter@reolmarked.dk", "Testvej 1, Middelby");

            // Tilføj nogle eksisterende kunder
            AddCustomer("Mette Larsen", "23456789", "mette@reolmarked.dk", "Middelby Hovedgade 12");
            AddCustomer("Lars Hansen", "34567890", "lars@email.dk", "Vestergade 15, Middelby");
            AddCustomer("Anna Nielsen", "45678901", "anna.nielsen@gmail.com", "Bakken 8, Middelby");
        }

        /// <summary>
        /// Henter alle kunder
        /// </summary>
        public ObservableCollection<Customer> GetAllCustomers()
        {
            return new ObservableCollection<Customer>(_customers);
        }

        /// <summary>
        /// Henter kun aktive kunder
        /// </summary>
        public ObservableCollection<Customer> GetActiveCustomers()
        {
            var activeCustomers = new List<Customer>();

            // Gå gennem alle kunder og find de aktive
            foreach (var customer in _customers)
            {
                if (customer.IsActive)
                {
                    activeCustomers.Add(customer);
                }
            }

            return new ObservableCollection<Customer>(activeCustomers);
        }

        /// <summary>
        /// Finder en kunde baseret på ID
        /// </summary>
        public Customer GetCustomerById(int customerId)
        {
            // Gå gennem alle kunder og find den med det rigtige ID
            foreach (var customer in _customers)
            {
                if (customer.CustomerId == customerId)
                {
                    return customer;
                }
            }

            // Returner null hvis kunden ikke findes
            return null;
        }

        /// <summary>
        /// Finder en kunde baseret på telefonnummer
        /// </summary>
        public Customer GetCustomerByPhone(string phoneNumber)
        {
            // Tjek at telefonnummer ikke er tomt
            if (string.IsNullOrEmpty(phoneNumber))
                return null;

            // Gå gennem alle kunder og find den med det rigtige telefonnummer
            foreach (var customer in _customers)
            {
                if (customer.CustomerPhone == phoneNumber)
                {
                    return customer;
                }
            }

            // Returner null hvis kunden ikke findes
            return null;
        }

        /// <summary>
        /// Tilføjer en ny kunde (som når Anton beslutter sig for at leje)
        /// </summary>
        public Customer AddCustomer(string name, string phone, string email, string address)
        {
            // Opret ny kunde
            var newCustomer = new Customer
            {
                CustomerId = _nextId++, // Tildel næste ID
                CustomerName = name,
                CustomerPhone = phone,
                CustomerEmail = email,
                CustomerAddress = address,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            // Tilføj til listen
            _customers.Add(newCustomer);

            // Returner den nye kunde
            return newCustomer;
        }

        /// <summary>
        /// Opdaterer en eksisterende kunde
        /// </summary>
        public bool UpdateCustomer(Customer customer)
        {
            // Find kunden i listen
            var existingCustomer = GetCustomerById(customer.CustomerId);

            if (existingCustomer != null)
            {
                // Opdater oplysningerne
                existingCustomer.CustomerName = customer.CustomerName;
                existingCustomer.CustomerPhone = customer.CustomerPhone;
                existingCustomer.CustomerEmail = customer.CustomerEmail;
                existingCustomer.CustomerAddress = customer.CustomerAddress;
                existingCustomer.IsActive = customer.IsActive;

                return true; // Opdatering lykkedes
            }

            return false; // Kunden blev ikke fundet
        }

        /// <summary>
        /// Deaktiverer en kunde (i stedet for at slette)
        /// </summary>
        public bool DeactivateCustomer(int customerId)
        {
            var customer = GetCustomerById(customerId);

            if (customer != null)
            {
                customer.IsActive = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Aktiverer en kunde igen
        /// </summary>
        public bool ActivateCustomer(int customerId)
        {
            var customer = GetCustomerById(customerId);

            if (customer != null)
            {
                customer.IsActive = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Søger efter kunder baseret på navn
        /// </summary>
        public ObservableCollection<Customer> SearchCustomersByName(string searchText)
        {
            var foundCustomers = new List<Customer>();

            // Tjek at søgetekst ikke er tom
            if (string.IsNullOrEmpty(searchText))
                return new ObservableCollection<Customer>(foundCustomers);

            // Gør søgning case-insensitive
            string searchLower = searchText.ToLower();

            // Gå gennem alle kunder og find matches
            foreach (var customer in _customers)
            {
                if (customer.CustomerName.ToLower().Contains(searchLower))
                {
                    foundCustomers.Add(customer);
                }
            }

            return new ObservableCollection<Customer>(foundCustomers);
        }

        /// <summary>
        /// Tæller antal aktive kunder
        /// </summary>
        public int CountActiveCustomers()
        {
            int count = 0;

            foreach (var customer in _customers)
            {
                if (customer.IsActive)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Tæller totalt antal kunder
        /// </summary>
        public int CountTotalCustomers()
        {
            return _customers.Count;
        }
    }
}