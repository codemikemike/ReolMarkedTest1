using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class SaleLineRepository : BaseRepository, ISaleLineRepository
    {
        public SaleLine Add(SaleLine saleLine)
        {
            const string sql = @"
                INSERT INTO SaleLine (SaleId, LabelId, Quantity, UnitPrice, LineTotal, AddedAt)
                OUTPUT INSERTED.SaleLineId
                VALUES (@SaleId, @LabelId, @Quantity, @UnitPrice, @LineTotal, @AddedAt)";

            var id = ExecuteScalar<int>(sql, saleLine);
            saleLine.SaleLineId = id;
            return saleLine;
        }

        public SaleLine? GetById(int id)
        {
            const string sql = "SELECT * FROM SaleLine WHERE SaleLineId = @id";
            return QuerySingleOrDefault<SaleLine>(sql, new { id });
        }

        public IEnumerable<SaleLine> GetAll()
        {
            const string sql = "SELECT * FROM SaleLine";
            return Query<SaleLine>(sql);
        }

        public void Update(SaleLine saleLine)
        {
            const string sql = @"
                UPDATE SaleLine 
                SET SaleId = @SaleId,
                    LabelId = @LabelId,
                    Quantity = @Quantity,
                    UnitPrice = @UnitPrice,
                    LineTotal = @LineTotal
                WHERE SaleLineId = @SaleLineId";

            Execute(sql, saleLine);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM SaleLine WHERE SaleLineId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<SaleLine> GetBySaleId(int saleId)
        {
            const string sql = "SELECT * FROM SaleLine WHERE SaleId = @saleId";
            return Query<SaleLine>(sql, new { saleId });
        }
    }
}