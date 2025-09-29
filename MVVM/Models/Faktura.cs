using System;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Opdateret Faktura model - nu med korrekt beregningslogik fra scenariet
    /// Følger Jonas og Sofies proces: Salg - Kommission - Næste måneds leje
    /// </summary>
    public class Faktura
    {
        // Eksisterende properties
        public int FakturaId { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPaid { get; set; }
        public string PaymentMethod { get; set; } = "";

        // Salgsdata
        public decimal TotalSales { get; set; }
        public decimal KommissionAmount { get; set; }

        // NYE PROPERTIES - manglede i original implementering
        public decimal NextMonthRent { get; set; }        // Næste måneds reolleje der trækkes fra
        public DateTime? PaymentDate { get; set; }        // Hvornår udbetaling/betaling skete
        public DateTime? BillSentDate { get; set; }       // Hvornår regning blev sendt (for røde tal)

        // Detaljer
        public ObservableCollection<RackSale> RackSales { get; set; } = new();
        public ObservableCollection<RentalAgreement> RentalAgreements { get; set; } = new();
        public ObservableCollection<Rack> CustomerRacks { get; set; } = new();  // NYE - kundens reoler

        // OPDATERET NetAmount beregning - nu korrekt efter scenariet
        private decimal _netAmount;
        public decimal NetAmount
        {
            get { return _netAmount; }
            set { _netAmount = value; }
        }

        /// <summary>
        /// Beregner slutbeløbet korrekt efter Jonas og Sofies metode:
        /// Samlet salg - 10% kommission - næste måneds reolleje
        /// </summary>
        public void CalculateNetAmount()
        {
            // Følger scenariet: "Jonas regner salget sammen og beregner derefter kommissionen 
            // for salg og trækker den fra, og herefter trækker han næste måneds leje"
            NetAmount = TotalSales - KommissionAmount - NextMonthRent;
        }

        /// <summary>
        /// Beregner 10% kommission af salget
        /// </summary>
        public void CalculateCommission()
        {
            KommissionAmount = TotalSales * 0.10m;
        }

        // Computed properties for UI
        public decimal CommissionRate => 0.10m;
        public string CustomerName => Customer?.CustomerName ?? "Ukendt kunde";

        // OPDATEREDE status properties
        public bool IsPositiveAmount => NetAmount > 0;     // Udbetaling nødvendig
        public bool IsNegativeAmount => NetAmount < 0;     // "Røde tal" - regning nødvendig
        public bool IsZeroAmount => NetAmount == 0;        // Lige op

        // Formattering til UI
        public string TotalSalesFormatted => $"{TotalSales:C0}";
        public string KommissionAmountFormatted => $"{KommissionAmount:C0}";
        public string NextMonthRentFormatted => $"{NextMonthRent:C0}";
        public string NetAmountFormatted => $"{NetAmount:C0}";

        // NYE - Forskellige visninger afhængig af om det er positivt/negativt beløb
        public string NetAmountDisplayFormatted
        {
            get
            {
                if (IsPositiveAmount)
                    return $"+{NetAmount:C0}";  // Grønt - udbetaling
                else if (IsNegativeAmount)
                    return $"{NetAmount:C0}";   // Rødt - skylder penge  
                else
                    return "0 kr";              // Lige op
            }
        }

        public string StatusText
        {
            get
            {
                if (!IsCompleted)
                    return "Ikke færdig";
                else if (IsPositiveAmount && !IsPaid)
                    return "Klar til udbetaling";
                else if (IsNegativeAmount && !IsPaid)
                    return "Regning sendt";
                else if (IsPaid)
                    return "Afsluttet";
                else if (IsZeroAmount)
                    return "Lige op - ingen handling";
                else
                    return "Ukendt status";
            }
        }

        // NYE - Hvad skal der ske med denne faktura?
        public string ActionRequired
        {
            get
            {
                if (!IsCompleted)
                    return "Skal færdiggøres";
                else if (IsPositiveAmount && !IsPaid)
                    return $"Udbetal {NetAmountFormatted}";
                else if (IsNegativeAmount && !IsPaid && !BillSentDate.HasValue)
                    return $"Send regning på {Math.Abs(NetAmount):C0}";
                else if (IsNegativeAmount && BillSentDate.HasValue && !IsPaid)
                    return $"Venter på betaling af {Math.Abs(NetAmount):C0}";
                else
                    return "Ingen handling nødvendig";
            }
        }

        // Factory method til at oprette månedlig faktura
        public static Faktura CreateMonthlyFaktura(Customer customer, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return new Faktura
            {
                CustomerId = customer.CustomerId,
                Customer = customer,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                CreatedAt = DateTime.Now,
                IsCompleted = false,
                IsPaid = false
            };
        }

        /// <summary>
        /// Marker fakturaen som færdig (alle beregninger er lavet)
        /// </summary>
        public void CompleteFaktura()
        {
            IsCompleted = true;
        }

        /// <summary>
        /// Marker som betalt/udbetalt
        /// </summary>
        public void MarkAsPaid()
        {
            IsPaid = true;
            PaymentDate = DateTime.Now;
        }

        /// <summary>
        /// Marker regning som sendt (for negative beløb)
        /// </summary>
        public void MarkBillSent()
        {
            BillSentDate = DateTime.Now;
        }

        // Periode beskrivelse
        public string PeriodDescription => $"{GetMonthName(PeriodStart.Month)} {PeriodStart.Year}";

        private string GetMonthName(int month)
        {
            string[] monthNames = {
                "", "Januar", "Februar", "Marts", "April", "Maj", "Juni",
                "Juli", "August", "September", "Oktober", "November", "December"
            };

            return month >= 1 && month <= 12 ? monthNames[month] : "Ukendt";
        }
    }
}