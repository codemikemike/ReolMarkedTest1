using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class LabelRepository : BaseRepository, ILabelRepository
    {
        public Label Add(Label label)
        {
            const string sql = @"
                INSERT INTO Label (ProductPrice, RackId, BarCode, SoldDate, CreatedAt, IsVoid)
                OUTPUT INSERTED.LabelId
                VALUES (@ProductPrice, @RackId, @BarCode, @SoldDate, @CreatedAt, @IsVoid)";

            label.CreatedAt = DateTime.Now;
            var id = ExecuteScalar<int>(sql, label);
            label.LabelId = id;
            return label;
        }

        public Label? GetById(int id)
        {
            const string sql = "SELECT * FROM Label WHERE LabelId = @id";
            return QuerySingleOrDefault<Label>(sql, new { id });
        }

        public IEnumerable<Label> GetAll()
        {
            const string sql = "SELECT * FROM Label";
            return Query<Label>(sql);
        }

        public void Update(Label label)
        {
            const string sql = @"
                UPDATE Label 
                SET ProductPrice = @ProductPrice,
                    RackId = @RackId,
                    BarCode = @BarCode,
                    SoldDate = @SoldDate,
                    IsVoid = @IsVoid
                WHERE LabelId = @LabelId";

            Execute(sql, label);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM Label WHERE LabelId = @id";
            Execute(sql, new { id });
        }

        public Label? GetByBarcode(string barcode)
        {
            const string sql = "SELECT * FROM Label WHERE BarCode = @barcode";
            return QuerySingleOrDefault<Label>(sql, new { barcode });
        }

        public IEnumerable<Label> GetByRackId(int rackId)
        {
            const string sql = "SELECT * FROM Label WHERE RackId = @rackId";
            return Query<Label>(sql, new { rackId });
        }

        public IEnumerable<Label> GetByCustomerId(int customerId)
        {
            // Labels har ikke direkte CustomerId i databasen
            // Du skal joine med Rack og RentalAgreement hvis du vil have dette
            return new List<Label>();
        }

        public IEnumerable<Label> GetActiveLabels()
        {
            const string sql = "SELECT * FROM Label WHERE IsVoid = 0 AND SoldDate IS NULL";
            return Query<Label>(sql);
        }

        public IEnumerable<Label> GetSoldLabels()
        {
            const string sql = "SELECT * FROM Label WHERE SoldDate IS NOT NULL";
            return Query<Label>(sql);
        }
    }
}