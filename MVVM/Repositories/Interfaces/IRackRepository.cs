// Repositories/Interfaces/IRackRepository.cs
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IRackRepository : IRepository<Rack>
    {
        Rack GetByRackNumber(int rackNumber);
        IEnumerable<Rack> GetByAvailability(bool isAvailable);
        IEnumerable<Rack> GetByHangerBarStatus(bool hasHangerBar);
    }
}