// Repositories/Interfaces/ISaleRepository.cs
using System;
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface ISaleRepository : IRepository<Sale>
    {
        IEnumerable<Sale> GetByDateRange(DateTime fromDate, DateTime toDate);
        IEnumerable<Sale> GetCompletedSales();
    }
}