using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        IEnumerable<Invoice> GetByCustomerId(int customerId);
        IEnumerable<Invoice> GetByPeriod(int year, int month);
        IEnumerable<Invoice> GetUnpaid();
    }
}