using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service klasse til at håndtere salg (UC3.1)
    /// Håndterer scanner funktionalitet og salgsprocessen som Jonas bruger
    /// </summary>
    public class SaleService
    {
        // Private lister til at gemme salgsdata
        private List<Sale> _sales;
        private List<RackSale> _rackSales;
        private int _nextSaleId;
        private int _nextSaleLineId;
        private int _nextRackSaleId;

        // Reference til andre services
        private BarcodeService _barcodeService;

        // Konstruktør - opretter service
        public SaleService(BarcodeService barcodeService)
        {
            _sales = new List<Sale>();
            _rackSales = new List<RackSale>();
            _nextSaleId = 1;
            _nextSaleLineId = 1;
            _nextRackSaleId = 1;
            _barcodeService = barcodeService;

            CreateTestBarcodes();
        }

        /// <summary>
        /// Opretter test stregkoder så scanner systemet har noget at arbejde med
        /// </summary>
        private void CreateTestBarcodes()
        {
            // Opret test stregkoder så scanner systemet har noget at scanne
            var peter = _barcodeService.FindCustomerByPhone("12345678");
            if (peter != null)
            {
                _barcodeService.CreateLabelForCustomer(peter.CustomerId, 7, 125.00m, "Keramikskål");
                _barcodeService.CreateLabelForCustomer(peter.CustomerId, 7, 45.00m, "Vintage bog");
                _barcodeService.CreateLabelForCustomer(peter.CustomerId, 42, 200.00m, "Antik vase");
            }

            var mette = _barcodeService.FindCustomerByPhone("23456789");
            if (mette != null)
            {
                _barcodeService.CreateLabelForCustomer(mette.CustomerId, 15, 85.00m, "Porcelænsfigur");
                _barcodeService.CreateLabelForCustomer(mette.CustomerId, 15, 150.00m, "Vintage taske");
            }
        }

        /// <summary>
        /// Starter et nyt salg (når Aya kommer til kassen)
        /// </summary>
        public Sale StartNewSale()
        {
            var newSale = new Sale
            {
                SaleId = _nextSaleId++,
                DatoTid = DateTime.Now,
                BetalingsForm = "Kontant"
            };

            _sales.Add(newSale);
            return newSale;
        }

        /// <summary>
        /// Scanner en stregkode og tilføjer produktet til salget (UC3.1 hovedfunktion)
        /// </summary>
        public ScanResult ScanBarcode(Sale sale, string barcode)
        {
            var result = new ScanResult();

            if (sale == null || string.IsNullOrEmpty(barcode))
            {
                result.Success = false;
                result.ErrorMessage = "Ugyldigt salg eller stregkode";
                return result;
            }

            // Find label baseret på stregkode
            var label = _barcodeService.FindLabelByBarcode(barcode);
            if (label == null)
            {
                result.Success = false;
                result.ErrorMessage = "Stregkode ikke fundet";
                return result;
            }

            // Tjek at produktet kan sælges
            if (label.IsSold)
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
                SalgsLinjeId = _nextSaleLineId++,
                SaleId = sale.SaleId,
                LabelId = label.LabelId,
                Label = label,
                Antal = 1,
                EnhedsPris = label.ProductPrice,
                AddedAt = DateTime.Now
            };

            saleLine.Sale = sale;

            // Tilføj til salget
            sale.AddSaleLine(saleLine);

            result.Success = true;
            result.AddedSaleLine = saleLine;
            result.Message = $"Tilføjet: {saleLine.ProductName} - {saleLine.EnhedsPrisFormatted}";

            return result;
        }

        /// <summary>
        /// Fjerner et produkt fra salget
        /// </summary>
        public bool RemoveProductFromSale(Sale sale, SaleLine saleLine)
        {
            if (sale == null || saleLine == null)
                return false;

            sale.RemoveSaleLine(saleLine);
            return true;
        }

        /// <summary>
        /// Gennemfører betaling og afslutter salget (når Aya betaler)
        /// </summary>
        public PaymentResult ProcessPayment(Sale sale, decimal betaltBelob, string betalingsForm)
        {
            var result = new PaymentResult();

            if (sale == null || sale.SaleLines.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "Intet at betale for";
                return result;
            }

            if (betaltBelob < sale.Total)
            {
                result.Success = false;
                result.ErrorMessage = $"Utilstrækkelig betaling. Mangler {(sale.Total - betaltBelob):C0}";
                return result;
            }

            // Opdater salg
            sale.BetaltBelob = betaltBelob;
            sale.BetalingsForm = betalingsForm;
            sale.CompleteSale();

            // Marker produkter som solgte og opret reol salg
            foreach (var saleLine in sale.SaleLines)
            {
                // Marker label som solgt
                if (saleLine.Label != null)
                {
                    saleLine.Label.MarkAsSold();
                }

                // Opret reol salg (Jonas' noter på reol kort)
                var rackSale = RackSale.CreateFromSaleLine(saleLine);
                if (rackSale != null)
                {
                    rackSale.ReolSalgId = _nextRackSaleId++;
                    _rackSales.Add(rackSale);
                }
            }

            result.Success = true;
            result.CompletedSale = sale;
            result.ByttePenge = sale.ByttePenge;
            result.Message = $"Betaling gennemført. Byttepenge: {sale.ByttePengeFormatted}";

            return result;
        }

        /// <summary>
        /// Annullerer et salg
        /// </summary>
        public bool CancelSale(Sale sale)
        {
            if (sale == null)
                return false;

            sale.CancelSale();
            return true;
        }

        // Eksisterende metoder for at hente data...
        public ObservableCollection<Sale> GetSalesForPeriod(DateTime fromDate, DateTime toDate)
        {
            var periodSales = new List<Sale>();

            foreach (var sale in _sales)
            {
                if (sale.DatoTid.Date >= fromDate.Date && sale.DatoTid.Date <= toDate.Date)
                {
                    periodSales.Add(sale);
                }
            }

            return new ObservableCollection<Sale>(periodSales);
        }

        public ObservableCollection<RackSale> GetRackSalesForCustomer(int customerId)
        {
            var customerRackSales = new List<RackSale>();

            foreach (var rackSale in _rackSales)
            {
                if (rackSale.CustomerId == customerId)
                {
                    customerRackSales.Add(rackSale);
                }
            }

            return new ObservableCollection<RackSale>(customerRackSales);
        }

        public ObservableCollection<RackSale> GetRackSalesForRack(int rackNumber)
        {
            var rackSalesList = new List<RackSale>();

            foreach (var rackSale in _rackSales)
            {
                if (rackSale.RackNumber == rackNumber)
                {
                    rackSalesList.Add(rackSale);
                }
            }

            return new ObservableCollection<RackSale>(rackSalesList);
        }

        public decimal CalculateTotalRevenue(DateTime fromDate, DateTime toDate)
        {
            decimal totalRevenue = 0;

            foreach (var sale in _sales)
            {
                if (sale.IsCompleted &&
                    sale.DatoTid.Date >= fromDate.Date &&
                    sale.DatoTid.Date <= toDate.Date)
                {
                    totalRevenue += sale.Total;
                }
            }

            return totalRevenue;
        }

        public Sale GetSaleById(int saleId)
        {
            foreach (var sale in _sales)
            {
                if (sale.SaleId == saleId)
                {
                    return sale;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Result klasse for scanner operationer
    /// </summary>
    public class ScanResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public SaleLine AddedSaleLine { get; set; }
    }

    /// <summary>
    /// Result klasse for betalings operationer
    /// </summary>
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = "";
        public string Message { get; set; } = "";
        public Sale CompletedSale { get; set; }
        public decimal ByttePenge { get; set; }
    }
}