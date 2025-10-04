using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RentalAgreementRepository : BaseRepository, IRentalAgreementRepository
    {
        public RentalAgreement Add(RentalAgreement agreement)
        {
            const string sql = @"
                INSERT INTO RentalAgreement (CustomerId, RackId, StartDate, MonthlyRent, Status, CreatedAt, Notes)
                OUTPUT INSERTED.AgreementId
                VALUES (@CustomerId, @RackId, @StartDate, @MonthlyRent, @Status, @CreatedAt, @Notes)";

            agreement.CreatedAt = DateTime.Now;
            var id = ExecuteScalar<int>(sql, agreement);
            agreement.AgreementId = id;
            return agreement;
        }

        public RentalAgreement? GetById(int id)
        {
            const string sql = "SELECT * FROM RentalAgreement WHERE AgreementId = @id";
            return QuerySingleOrDefault<RentalAgreement>(sql, new { id });
        }

        public IEnumerable<RentalAgreement> GetAll()
        {
            const string sql = "SELECT * FROM RentalAgreement";
            return Query<RentalAgreement>(sql);
        }

        public void Update(RentalAgreement agreement)
        {
            const string sql = @"
                UPDATE RentalAgreement 
                SET CustomerId = @CustomerId,
                    RackId = @RackId,
                    StartDate = @StartDate,
                    MonthlyRent = @MonthlyRent,
                    Status = @Status,
                    Notes = @Notes
                WHERE AgreementId = @AgreementId";

            Execute(sql, agreement);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM RentalAgreement WHERE AgreementId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<RentalAgreement> GetByCustomerId(int customerId)
        {
            const string sql = "SELECT * FROM RentalAgreement WHERE CustomerId = @customerId";
            return Query<RentalAgreement>(sql, new { customerId });
        }

        public IEnumerable<RentalAgreement> GetByRackId(int rackId)
        {
            const string sql = "SELECT * FROM RentalAgreement WHERE RackId = @rackId";
            return Query<RentalAgreement>(sql, new { rackId });
        }

        public IEnumerable<RentalAgreement> GetByStatus(RentalStatus status)
        {
            const string sql = "SELECT * FROM RentalAgreement WHERE Status = @status";
            return Query<RentalAgreement>(sql, new { status = (int)status });
        }

        public RentalAgreement? GetActiveAgreementForRack(int rackId)
        {
            const string sql = "SELECT * FROM RentalAgreement WHERE RackId = @rackId AND Status = 0";
            return QuerySingleOrDefault<RentalAgreement>(sql, new { rackId });
        }
    }
}