// Repositories/Interfaces/ISaleLineRepository.cs
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface ISaleLineRepository : IRepository<SaleLine>
    {
        IEnumerable<SaleLine> GetBySaleId(int saleId);
    }
}