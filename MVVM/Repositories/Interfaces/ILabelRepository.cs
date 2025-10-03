// Repositories/Interfaces/ILabelRepository.cs
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface ILabelRepository : IRepository<Label>
    {
        Label GetByBarcode(string barcode);
        IEnumerable<Label> GetByRackId(int rackId);
        IEnumerable<Label> GetByCustomerId(int customerId);
        IEnumerable<Label> GetActiveLabels();
        IEnumerable<Label> GetSoldLabels();
    }
}