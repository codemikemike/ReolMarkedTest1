// Repositories/Interfaces/IRackSaleRepository.cs
using System;
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IRackSaleRepository : IRepository<RackSale>
    {
        IEnumerable<RackSale> GetByCustomerId(int customerId);
        IEnumerable<RackSale> GetByRackNumber(int rackNumber);
        IEnumerable<RackSale> GetByDateRange(DateTime fromDate, DateTime toDate);
    }
}