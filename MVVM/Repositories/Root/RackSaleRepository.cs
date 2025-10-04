using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackSaleRepository : BaseRepository, IRackSaleRepository
    {
        public RackSale Add(RackSale rackSale)
        {
            const string sql = @"
                INSERT INTO RackSale (SaleId, RackNumber, CustomerId, Date, Amount, ProductInfo, Notes)
                OUTPUT INSERTED.RackSaleId
                VALUES (@SaleId, @RackNumber, @CustomerId, @Date, @Amount, @ProductInfo, @Notes)";

            var id = ExecuteScalar<int>(sql, rackSale);
            rackSale.RackSaleId = id;
            return rackSale;
        }

        public RackSale? GetById(int id)
        {
            const string sql = "SELECT * FROM RackSale WHERE RackSaleId = @id";
            return QuerySingleOrDefault<RackSale>(sql, new { id });
        }

        public IEnumerable<RackSale> GetAll()
        {
            const string sql = "SELECT * FROM RackSale ORDER BY Date DESC";
            return Query<RackSale>(sql);
        }

        public void Update(RackSale rackSale)
        {
            const string sql = @"
                UPDATE RackSale 
                SET SaleId = @SaleId,
                    RackNumber = @RackNumber,
                    CustomerId = @CustomerId,
                    Date = @Date,
                    Amount = @Amount,
                    ProductInfo = @ProductInfo,
                    Notes = @Notes
                WHERE RackSaleId = @RackSaleId";

            Execute(sql, rackSale);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM RackSale WHERE RackSaleId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<RackSale> GetByCustomerId(int customerId)
        {
            const string sql = "SELECT * FROM RackSale WHERE CustomerId = @customerId ORDER BY Date DESC";
            return Query<RackSale>(sql, new { customerId });
        }

        public IEnumerable<RackSale> GetByRackNumber(int rackNumber)
        {
            const string sql = "SELECT * FROM RackSale WHERE RackNumber = @rackNumber ORDER BY Date DESC";
            return Query<RackSale>(sql, new { rackNumber });
        }

        public IEnumerable<RackSale> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            const string sql = @"
                SELECT * FROM RackSale 
                WHERE Date >= @fromDate AND Date <= @toDate
                ORDER BY Date DESC";

            return Query<RackSale>(sql, new { fromDate = fromDate.Date, toDate = toDate.Date });
        }
    }
}