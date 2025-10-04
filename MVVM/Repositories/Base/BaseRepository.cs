using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ReolMarked.MVVM.Repositories.Base
{
    /// <summary>
    /// Base repository klasse med fælles database funktionalitet
    /// </summary>
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository()
        {
            _connectionString = Database.DatabaseConfiguration.Instance.ConnectionString;
        }

        /// <summary>
        /// Opretter en ny database forbindelse
        /// </summary>
        protected IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Eksekverer en query og returnerer resultater
        /// </summary>
        protected async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Eksekverer en query og returnerer et enkelt resultat
        /// </summary>
        protected async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Eksekverer en query og returnerer første resultat
        /// </summary>
        protected async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Eksekverer en command (INSERT, UPDATE, DELETE)
        /// </summary>
        protected async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(sql, parameters);
        }

        /// <summary>
        /// Eksekverer en scalar query (COUNT, SUM, etc.)
        /// </summary>
        protected async Task<T> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<T>(sql, parameters);
        }

        /// <summary>
        /// Synkrone versioner for backwards compatibility
        /// </summary>
        protected IEnumerable<T> Query<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return connection.Query<T>(sql, parameters);
        }

        protected T? QuerySingleOrDefault<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return connection.QuerySingleOrDefault<T>(sql, parameters);
        }

        protected int Execute(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return connection.Execute(sql, parameters);
        }

        protected T ExecuteScalar<T>(string sql, object? parameters = null)
        {
            using var connection = CreateConnection();
            return connection.ExecuteScalar<T>(sql, parameters);
        }

        /// <summary>
        /// Starter en database transaktion
        /// </summary>
        protected async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> operation)
        {
            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var result = await operation(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}