using System;
using System.Collections.Generic;
using System.Linq;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services.Results;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere salg
    /// AL forretningslogik for scanner og salgsprocessen
    /// </summary>
    public class SaleService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly ISaleLineRepository _saleLineRepository;
        private readonly IRackSaleRepository _rackSaleRepository;
        private readonly BarcodeService _barcodeService;

        public SaleService(
            ISaleRepository saleRepository,
            ISaleLineRepository saleLineRepository,
            IRackSaleRepository rackSaleRepository,
            BarcodeService barcodeService)
        {
            _saleRepository = saleRepository;
            _saleLineRepository = saleLineRepository;
            _rackSaleRepository = rackSaleRepository;
            _barcodeService = barcodeService;
        }

        /// <summary>
        /// Starter et nyt salg
        /// </summary>
        public Sale StartNewSale()
        {
            var sale = new Sale
            {
                SaleDateTime = DateTime.Now,
                PaymentMethod = PaymentMethod.Cash,
                IsCompleted = false,
                Total = 0,
                AmountPaid = 0,
                ChangeGiven = 0
            };

            return _saleRepository.Add(sale);
        }

        /// <summary>
        /// Scanner en stregkode og tilføjer produktet til salget
        /// </summary>
        public ScanResult ScanBarcode(int saleId, string barcode)
        {
            var result = new ScanResult();
            var sale = _saleRepository.GetById(saleId);

            if (sale == null || string.IsNullOrEmpty(barcode))
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldigt salg eller stregkode";
                return result;
            }

            // Find label
            var label = _barcodeService.FindLabelByBarcode(barcode);
            if (label == null)
            {
                result.Success = false;
                result.ErrorMessage = "Stregkode ikke fundet";
                return result;
            }

            // Valider label
            if (label.SoldDate.HasValue)
            {
                result.Success = false;
                result.ErrorMessage = "Produktet er allerede solgt";
                return result;
            }

            if (label.IsVoid)
            {
                result.Success = false;
                result.ErrorMessage = "Produktet er annulleret";
                return result;
            }

            // Opret salgslinje
            var saleLine = new SaleLine
            {
                SaleId = saleId,
                LabelId = label.LabelId,
                Quantity = 1,
                UnitPrice = label.ProductPrice,
                LineTotal = label.ProductPrice,
                AddedAt = DateTime.Now
            };

            saleLine = _saleLineRepository.Add(saleLine);
            saleLine.Label = label;
            saleLine.Sale = sale;

            // Opdater total
            CalculateAndUpdateSaleTotal(saleId);

            result.Success = true;
            result.AddedSaleLine = saleLine;
            result.Message = $"Tilføjet: {label.ProductPrice:C0}";

            return result;
        }

        /// <summary>
        /// Beregner linjetotal for en salgslinje
        /// </summary>
        private decimal CalculateLineTotal(int quantity, decimal unitPrice)
        {
            return quantity * unitPrice;
        }

        /// <summary>
        /// Beregner og opdaterer total for salg
        /// </summary>
        private void CalculateAndUpdateSaleTotal(int saleId)
        {
            var saleLines = _saleLineRepository.GetBySaleId(saleId);
            var total = saleLines.Sum(sl => sl.LineTotal);

            var sale = _saleRepository.GetById(saleId);
            if (sale != null)
            {
                sale.Total = total;
                _saleRepository.Update(sale);
            }
        }

        /// <summary>
        /// Fjerner et produkt fra salget
        /// </summary>
        public bool RemoveProductFromSale(int saleId, int saleLineId)
        {
            var saleLine = _saleLineRepository.GetById(saleLineId);
            if (saleLine == null || saleLine.SaleId != saleId)
                return false;

            _saleLineRepository.Delete(saleLineId);
            CalculateAndUpdateSaleTotal(saleId);
            return true;
        }

        /// <summary>
        /// Gennemfører betaling og afslutter salget
        /// </summary>
        public PaymentResult ProcessPayment(int saleId, decimal amountPaid, PaymentMethod paymentMethod)
        {
            var result = new PaymentResult();
            var sale = _saleRepository.GetById(saleId);

            if (sale == null)
            {
                result.Success = false;
                result.ErrorMessage = "Salg ikke fundet";
                return result;
            }

            var saleLines = _saleLineRepository.GetBySaleId(saleId).ToList();
            if (saleLines.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Intet at betale for";
                return result;
            }

            if (amountPaid < sale.Total)
            {
                result.Success = false;
                result.ErrorMessage = $"Utilstrækkelig betaling. Mangler {(sale.Total - amountPaid):C0}";
                return result;
            }

            // Opdater salg
            sale.AmountPaid = amountPaid;
            sale.PaymentMethod = paymentMethod;
            sale.ChangeGiven = amountPaid - sale.Total;
            sale.IsCompleted = true;
            sale.SaleDateTime = DateTime.Now;
            _saleRepository.Update(sale);

            // Marker produkter som solgte og opret reol salg
            foreach (var saleLine in saleLines)
            {
                // Marker label som solgt
                if (saleLine.LabelId > 0)
                {
                    _barcodeService.MarkLabelAsSold(saleLine.LabelId);
                }

                // Opret reol salg
                var label = _barcodeService.FindLabelByBarcode(saleLine.Label?.BarCode ?? "");
                if (label?.Customer != null)
                {
                    var rackSale = new RackSale
                    {
                        SaleId = saleId,
                        RackNumber = label.RackId,
                        CustomerId = label.Customer.CustomerId,
                        Date = DateTime.Now,
                        Amount = saleLine.LineTotal,
                        ProductInfo = $"{label.BarCode}",
                        Notes = $"Salg via scanner - {saleLine.Quantity} stk"
                    };

                    _rackSaleRepository.Add(rackSale);
                }
            }

            result.Success = true;
            result.CompletedSale = sale;
            result.ChangeGiven = sale.ChangeGiven;
            result.Message = $"Betaling gennemført. Byttepenge: {sale.ChangeGiven:C0}";

            return result;
        }

        /// <summary>
        /// Annullerer et salg
        /// </summary>
        public bool CancelSale(int saleId)
        {
            var sale = _saleRepository.GetById(saleId);
            if (sale == null)
                return false;

            // Slet alle salgslinjer
            var saleLines = _saleLineRepository.GetBySaleId(saleId);
            foreach (var saleLine in saleLines)
            {
                _saleLineRepository.Delete(saleLine.SaleLineId);
            }

            // Opdater salg
            sale.Total = 0;
            sale.AmountPaid = 0;
            sale.ChangeGiven = 0;
            sale.IsCompleted = false;
            sale.Notes = "Annulleret";
            _saleRepository.Update(sale);

            return true;
        }

        /// <summary>
        /// Henter salg for en periode
        /// </summary>
        public IEnumerable<Sale> GetSalesForPeriod(DateTime fromDate, DateTime toDate)
        {
            return _saleRepository.GetByDateRange(fromDate, toDate);
        }

        /// <summary>
        /// Henter reol salg for en kunde
        /// </summary>
        public IEnumerable<RackSale> GetRackSalesForCustomer(int customerId)
        {
            return _rackSaleRepository.GetByCustomerId(customerId);
        }

        /// <summary>
        /// Beregner total omsætning for periode
        /// </summary>
        public decimal CalculateTotalRevenue(DateTime fromDate, DateTime toDate)
        {
            var sales = _saleRepository.GetByDateRange(fromDate, toDate)
                .Where(s => s.IsCompleted);

            return sales.Sum(s => s.Total);
        }

        public Sale GetSaleById(int saleId)
        {
            return _saleRepository.GetById(saleId);
        }
    }
}