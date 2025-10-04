using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    /// <summary>
    /// Repository til håndtering af Customer data i databasen
    /// </summary>
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository() : base()
        {
        }

        /// <summary>
        /// Tilføjer en ny kunde til databasen
        /// </summary>
        public Customer Add(Customer customer)
        {
            const string sql = @"
                INSERT INTO Customer (Name, Phone, Email, Address, CreatedAt, IsActive)
                OUTPUT INSERTED.CustomerId
                VALUES (@Name, @Phone, @Email, @Address, @CreatedAt, @IsActive)";

            customer.CreatedAt = DateTime.Now;
            customer.IsActive = true;

            var id = ExecuteScalar<int>(sql, customer);
            customer.CustomerId = id;

            return customer;
        }

        /// <summary>
        /// Henter en kunde baseret på ID
        /// </summary>
        public Customer? GetById(int id)
        {
            const string sql = "SELECT * FROM Customer WHERE CustomerId = @id";
            return QuerySingleOrDefault<Customer>(sql, new { id });
        }

        /// <summary>
        /// Henter alle kunder
        /// </summary>
        public IEnumerable<Customer> GetAll()
        {
            const string sql = "SELECT * FROM Customer ORDER BY Name";
            return Query<Customer>(sql);
        }

        /// <summary>
        /// Opdaterer en eksisterende kunde
        /// </summary>
        public void Update(Customer customer)
        {
            const string sql = @"
                UPDATE Customer 
                SET Name = @Name, 
                    Phone = @Phone, 
                    Email = @Email, 
                    Address = @Address, 
                    IsActive = @IsActive
                WHERE CustomerId = @CustomerId";

            Execute(sql, customer);
        }

        /// <summary>
        /// Sletter en kunde (soft delete - sætter IsActive til false)
        /// </summary>
        public void Delete(int id)
        {
            const string sql = "UPDATE Customer SET IsActive = 0 WHERE CustomerId = @id";
            Execute(sql, new { id });
        }

        /// <summary>
        /// Finder en kunde baseret på telefonnummer
        /// </summary>
        public Customer? GetByPhone(string phone)
        {
            const string sql = "SELECT * FROM Customer WHERE Phone = @phone";
            return QuerySingleOrDefault<Customer>(sql, new { phone });
        }

        /// <summary>
        /// Henter kunder baseret på status
        /// </summary>
        public IEnumerable<Customer> GetByStatus(bool isActive)
        {
            const string sql = "SELECT * FROM Customer WHERE IsActive = @isActive ORDER BY Name";
            return Query<Customer>(sql, new { isActive });
        }

        /// <summary>
        /// Søger efter kunder baseret på navn
        /// </summary>
        public IEnumerable<Customer> SearchByName(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return Enumerable.Empty<Customer>();

            const string sql = "SELECT * FROM Customer WHERE Name LIKE @searchPattern ORDER BY Name";
            var searchPattern = $"%{searchText}%";
            return Query<Customer>(sql, new { searchPattern });
        }
    }
}