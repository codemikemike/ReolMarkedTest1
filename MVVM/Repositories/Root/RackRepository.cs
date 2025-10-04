using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class RackRepository : BaseRepository, IRackRepository
    {
        public Rack Add(Rack rack)
        {
            const string sql = @"
                INSERT INTO Rack (RackNumber, HasHangerBar, AmountShelves, Location, IsAvailable, Description)
                OUTPUT INSERTED.RackId
                VALUES (@RackNumber, @HasHangerBar, @AmountShelves, @Location, @IsAvailable, @Description)";

            var id = ExecuteScalar<int>(sql, rack);
            rack.RackId = id;
            return rack;
        }

        public Rack? GetById(int id)
        {
            const string sql = "SELECT * FROM Rack WHERE RackId = @id";
            return QuerySingleOrDefault<Rack>(sql, new { id });
        }

        public IEnumerable<Rack> GetAll()
        {
            const string sql = "SELECT * FROM Rack ORDER BY RackNumber";
            return Query<Rack>(sql);
        }

        public void Update(Rack rack)
        {
            const string sql = @"
                UPDATE Rack 
                SET RackNumber = @RackNumber,
                    HasHangerBar = @HasHangerBar,
                    AmountShelves = @AmountShelves,
                    Location = @Location,
                    IsAvailable = @IsAvailable,
                    Description = @Description
                WHERE RackId = @RackId";

            Execute(sql, rack);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM Rack WHERE RackId = @id";
            Execute(sql, new { id });
        }

        public Rack? GetByRackNumber(int rackNumber)
        {
            const string sql = "SELECT * FROM Rack WHERE RackNumber = @rackNumber";
            return QuerySingleOrDefault<Rack>(sql, new { rackNumber });
        }

        public IEnumerable<Rack> GetByAvailability(bool isAvailable)
        {
            const string sql = "SELECT * FROM Rack WHERE IsAvailable = @isAvailable ORDER BY RackNumber";
            return Query<Rack>(sql, new { isAvailable });
        }

        public IEnumerable<Rack> GetByHangerBarStatus(bool hasHangerBar)
        {
            const string sql = "SELECT * FROM Rack WHERE HasHangerBar = @hasHangerBar ORDER BY RackNumber";
            return Query<Rack>(sql, new { hasHangerBar });
        }
    }
}