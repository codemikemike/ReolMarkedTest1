using System.Collections.Generic;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Repositories.Interfaces
{
    public interface IRentalAgreementRepository : IRepository<RentalAgreement>
    {
        IEnumerable<RentalAgreement> GetByCustomerId(int customerId);
        IEnumerable<RentalAgreement> GetByRackId(int rackId);
        IEnumerable<RentalAgreement> GetByStatus(RentalStatus status);
        RentalAgreement GetActiveAgreementForRack(int rackId);
    }
}