using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackTerminationRepository : BaseRepository, IRackTerminationRepository
    {
        public RackTermination Add(RackTermination termination)
        {
            const string sql = @"
                INSERT INTO RackTermination (AgreementId, CustomerId, RackNumber, RequestDate, 
                                           EffectiveDate, CreatedAt, Reason, Notes, IsProcessed)
                OUTPUT INSERTED.TerminationId
                VALUES (@AgreementId, @CustomerId, @RackNumber, @RequestDate,
                        @EffectiveDate, @CreatedAt, @Reason, @Notes, @IsProcessed)";

            termination.CreatedAt = DateTime.Now;
            var id = ExecuteScalar<int>(sql, termination);
            termination.TerminationId = id;
            return termination;
        }

        public RackTermination? GetById(int id)
        {
            const string sql = "SELECT * FROM RackTermination WHERE TerminationId = @id";
            return QuerySingleOrDefault<RackTermination>(sql, new { id });
        }

        public IEnumerable<RackTermination> GetAll()
        {
            const string sql = "SELECT * FROM RackTermination ORDER BY CreatedAt DESC";
            return Query<RackTermination>(sql);
        }

        public void Update(RackTermination termination)
        {
            const string sql = @"
                UPDATE RackTermination 
                SET AgreementId = @AgreementId,
                    CustomerId = @CustomerId,
                    RackNumber = @RackNumber,
                    RequestDate = @RequestDate,
                    EffectiveDate = @EffectiveDate,
                    Reason = @Reason,
                    Notes = @Notes,
                    IsProcessed = @IsProcessed
                WHERE TerminationId = @TerminationId";

            Execute(sql, termination);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM RackTermination WHERE TerminationId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<RackTermination> GetByCustomerId(int customerId)
        {
            const string sql = "SELECT * FROM RackTermination WHERE CustomerId = @customerId ORDER BY CreatedAt DESC";
            return Query<RackTermination>(sql, new { customerId });
        }

        public IEnumerable<RackTermination> GetByAgreementId(int agreementId)
        {
            const string sql = "SELECT * FROM RackTermination WHERE AgreementId = @agreementId";
            return Query<RackTermination>(sql, new { agreementId });
        }

        public IEnumerable<RackTermination> GetActive()
        {
            const string sql = "SELECT * FROM RackTermination WHERE EffectiveDate > GETDATE() AND IsProcessed = 0";
            return Query<RackTermination>(sql);
        }

        public IEnumerable<RackTermination> GetEffectiveBeforeDate(DateTime date)
        {
            const string sql = @"
                SELECT * FROM RackTermination 
                WHERE EffectiveDate <= @date AND IsProcessed = 1
                ORDER BY EffectiveDate";

            return Query<RackTermination>(sql, new { date = date.Date });
        }
    }
}