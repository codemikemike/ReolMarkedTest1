using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Base;
using ReolMarked.MVVM.Repositories.Interfaces;

namespace ReolMarked.MVVM.Repositories
{
    public class InvoiceRepository : BaseRepository, IInvoiceRepository
    {
        public Invoice Add(Invoice invoice)
        {
            const string sql = @"
                INSERT INTO Invoice (CustomerId, PeriodStart, PeriodEnd, CreatedAt, IsCompleted, IsPaid, 
                                   PaymentMethod, TotalSales, CommissionAmount, NextMonthRent, NetAmount, 
                                   PaymentDate, InvoiceSentDate)
                OUTPUT INSERTED.InvoiceId
                VALUES (@CustomerId, @PeriodStart, @PeriodEnd, @CreatedAt, @IsCompleted, @IsPaid,
                        @PaymentMethod, @TotalSales, @CommissionAmount, @NextMonthRent, @NetAmount,
                        @PaymentDate, @InvoiceSentDate)";

            invoice.CreatedAt = DateTime.Now;
            var id = ExecuteScalar<int>(sql, invoice);
            invoice.InvoiceId = id;
            return invoice;
        }

        public Invoice? GetById(int id)
        {
            const string sql = "SELECT * FROM Invoice WHERE InvoiceId = @id";
            return QuerySingleOrDefault<Invoice>(sql, new { id });
        }

        public IEnumerable<Invoice> GetAll()
        {
            const string sql = "SELECT * FROM Invoice ORDER BY CreatedAt DESC";
            return Query<Invoice>(sql);
        }

        public void Update(Invoice invoice)
        {
            const string sql = @"
                UPDATE Invoice 
                SET CustomerId = @CustomerId,
                    PeriodStart = @PeriodStart,
                    PeriodEnd = @PeriodEnd,
                    IsCompleted = @IsCompleted,
                    IsPaid = @IsPaid,
                    PaymentMethod = @PaymentMethod,
                    TotalSales = @TotalSales,
                    CommissionAmount = @CommissionAmount,
                    NextMonthRent = @NextMonthRent,
                    NetAmount = @NetAmount,
                    PaymentDate = @PaymentDate,
                    InvoiceSentDate = @InvoiceSentDate
                WHERE InvoiceId = @InvoiceId";

            Execute(sql, invoice);
        }

        public void Delete(int id)
        {
            const string sql = "DELETE FROM Invoice WHERE InvoiceId = @id";
            Execute(sql, new { id });
        }

        public IEnumerable<Invoice> GetByCustomerId(int customerId)
        {
            const string sql = "SELECT * FROM Invoice WHERE CustomerId = @customerId ORDER BY CreatedAt DESC";
            return Query<Invoice>(sql, new { customerId });
        }

        public IEnumerable<Invoice> GetByPeriod(int year, int month)
        {
            const string sql = @"
                SELECT * FROM Invoice 
                WHERE YEAR(PeriodStart) = @year AND MONTH(PeriodStart) = @month
                ORDER BY CreatedAt DESC";
            return Query<Invoice>(sql, new { year, month });
        }

        public IEnumerable<Invoice> GetUnpaid()
        {
            const string sql = "SELECT * FROM Invoice WHERE IsPaid = 0 AND IsCompleted = 1 ORDER BY CreatedAt";
            return Query<Invoice>(sql);
        }
    }
}