// Repositories/Interfaces/ICustomerRepository.cs
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Customer GetByPhone(string phone);
        IEnumerable<Customer> GetByStatus(bool isActive);
        IEnumerable<Customer> SearchByName(string searchText);
    }
}