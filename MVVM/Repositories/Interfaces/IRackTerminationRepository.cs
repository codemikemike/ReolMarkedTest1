// Repositories/Interfaces/IRackTerminationRepository.cs
using System;
using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IRackTerminationRepository : IRepository<RackTermination>
    {
        IEnumerable<RackTermination> GetByCustomerId(int customerId);
        IEnumerable<RackTermination> GetByAgreementId(int agreementId);
        IEnumerable<RackTermination> GetActive();
        IEnumerable<RackTermination> GetEffectiveBeforeDate(DateTime date);
    }
}