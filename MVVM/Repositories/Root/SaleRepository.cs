using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class SaleRepository : BaseRepository, ISaleRepository
    {
        public Sale Add(Sale sale)
        {
            const string sql = @"
                INSERT INTO Sale (SaleDateTime, Total, PaymentMethod, AmountPaid, ChangeGiven, IsCompleted, Notes)
                OUTPUT INSERTED.SaleId
                VALUES (@SaleDateTime, @Total, @PaymentMethod, @AmountPaid, @ChangeGiven, @IsCompleted, @Notes)";

            var id = ExecuteScalar<int>(sql, sale);
            sale.SaleId = id;
            return sale;
        }

        public Sale? GetById(int id)
        {
            const string sql = "SELECT * FROM Sale WHERE SaleId = @id";
            return QuerySingleOrDefault<Sale>(sql, new { id });
        }

        public IEnumerable<Sale> GetAll()
        {
            const string sql = "SELECT * FROM Sale ORDER BY SaleDateTime DESC";
            return Query<Sale>(sql);
        }

        public void Update(Sale sale)
        {
            const string sql = @"
                UPDATE Sale 
                SET SaleDateTime = @SaleDateTime,
                    Total = @Total,
                    PaymentMethod = @PaymentMethod,
                    AmountPaid = @AmountPaid,
                    ChangeGiven = @ChangeGiven,
                    IsCompleted = @IsCompleted,
                    Notes = @Notes
                WHERE SaleId = @SaleId";

            Execute(sql, sale);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM Sale WHERE SaleId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<Sale> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
                SELECT * FROM Sale 
                WHERE CAST(SaleDateTime AS DATE) >= @fromDate 
                  AND CAST(SaleDateTime AS DATE) <= @toDate
                ORDER BY SaleDateTime DESC";

            return Query<Sale>(sql, new { fromDate = fromDate.Date, toDate = toDate.Date });
        }

        public IEnumerable<Sale> GetCompletedSales()
        {
            const string sql = "SELECT * FROM Sale WHERE IsCompleted = 1 ORDER BY SaleDateTime DESC";
            return Query<Sale>(sql);
        }
    }
}