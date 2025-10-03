
using ReolMarked.MVVM.Infrastructure;
using ReolMarked.MVVM.Models;

namespace ReolMarked.MVVM.Data
{
    public static class TestDataInitializer
    {
        public static void InitializeTestData()
        {


            // Initialize racks first
            InitializeRacks();

            // Then customers
            InitializeCustomers();

            // Then rental agreements
            InitializeRentalAgreements();

            // Then some sample sales
            InitializeSampleSales();
        }

        private static void InitializeRacks()
        {
            var rackRepo = ServiceLocator.RackRepository;

            // Tjek om der allerede er reoler
            if (rackRepo.GetAll().Any())
            {
                return; // Allerede initialiseret
            }

            // Opret alle 80 reoler
            for (int i = 1; i <= 80; i++)
            {
                var rack = new Rack
                {
                    RackId = i,
                    RackNumber = i,
                    IsAvailable = true,
                    AmountShelves = 6,
                    HasHangerBar = (i % 5 == 0),
                    Location = GetRackLocation(i),
                    Description = $"Reol {i}"
                };

                rackRepo.Add(rack);
            }
        }

       

        private static string GetRackLocation(int rackNumber)
        {
            // Fordel reoler på forskellige gange
            if (rackNumber <= 20)
                return "Gang A";
            else if (rackNumber <= 40)
                return "Gang B";
            else if (rackNumber <= 60)
                return "Gang C";
            else
                return "Gang D";
        }

        private static void InitializeCustomers()
        {
            var customerService = ServiceLocator.CustomerService;

            // Opret nogle test kunder (fra scenarierne)
            customerService.CreateCustomer(
                "Peter Holm",
                "12345678",
                "peter@example.com",
                "Hovedgaden 15, 4000 Middelby"
            );

            customerService.CreateCustomer(
                "Louise Hansen",
                "87654321",
                "louise@example.com",
                "Parkvej 8, 4000 Middelby"
            );

            customerService.CreateCustomer(
                "Anton Mikkelsen",
                "11223344",
                "anton@example.com",
                "Skovvej 22, 4000 Middelby"
            );

            customerService.CreateCustomer(
                "Aya Nielsen",
                "99887766",
                "aya@example.com",
                "Strandvejen 5, 4000 Middelby"
            );

            customerService.CreateCustomer(
                "Sofie Jensen",
                "55443322",
                "sofie@example.com",
                "Kirkevej 12, 4000 Middelby"
            );
        }

        private static void InitializeRentalAgreements()
        {
            var rentalService = ServiceLocator.RentalService;
            var customerRepo = ServiceLocator.CustomerRepository;

            // Peter Holm har reol 7 og 42 (fra scenario)
            var peter = customerRepo.GetByPhone("12345678");
            if (peter != null)
            {
                rentalService.CreateRentalAgreement(peter.CustomerId, 7, DateTime.Now.AddMonths(-3));
                rentalService.CreateRentalAgreement(peter.CustomerId, 42, DateTime.Now.AddMonths(-2));
            }

            // Louise har reol 54 og 55 (fra scenario)
            var louise = customerRepo.GetByPhone("87654321");
            if (louise != null)
            {
                rentalService.CreateRentalAgreement(louise.CustomerId, 54, DateTime.Now.AddMonths(-4));
                rentalService.CreateRentalAgreement(louise.CustomerId, 55, DateTime.Now.AddMonths(-4));
            }

            // Sofie har nogle reoler
            var sofie = customerRepo.GetByPhone("55443322");
            if (sofie != null)
            {
                rentalService.CreateRentalAgreement(sofie.CustomerId, 12, DateTime.Now.AddMonths(-1));
                rentalService.CreateRentalAgreement(sofie.CustomerId, 13, DateTime.Now.AddMonths(-1));
                rentalService.CreateRentalAgreement(sofie.CustomerId, 14, DateTime.Now.AddMonths(-1));
            }
        }

        private static void InitializeSampleSales()
        {
            var barcodeService = ServiceLocator.BarcodeService;
            var saleService = ServiceLocator.SaleService;
            var customerRepo = ServiceLocator.CustomerRepository;

            // Opret nogle labels for Peter's reoler
            var peter = customerRepo.GetByPhone("12345678");
            if (peter != null)
            {
                // Opret labels for reol 7
                var label1 = barcodeService.CreateLabelForCustomer(peter.CustomerId, 7, 125m);
                var label2 = barcodeService.CreateLabelForCustomer(peter.CustomerId, 7, 85m);
                var label3 = barcodeService.CreateLabelForCustomer(peter.CustomerId, 7, 200m);

                // Opret labels for reol 42
                var label4 = barcodeService.CreateLabelForCustomer(peter.CustomerId, 42, 150m);
                var label5 = barcodeService.CreateLabelForCustomer(peter.CustomerId, 42, 95m);

                // Simuler et salg
                var sale = saleService.StartNewSale();
                if (label1 != null)
                {
                    saleService.ScanBarcode(sale.SaleId, label1.BarCode);
                }
                if (label2 != null)
                {
                    saleService.ScanBarcode(sale.SaleId, label2.BarCode);
                }
                saleService.ProcessPayment(sale.SaleId, 250m, PaymentMethod.MobilePay);
            }

            // Opret nogle labels for Louise's reoler
            var louise = customerRepo.GetByPhone("87654321");
            if (louise != null)
            {
                barcodeService.CreateLabelForCustomer(louise.CustomerId, 54, 175m);
                barcodeService.CreateLabelForCustomer(louise.CustomerId, 54, 225m);
                barcodeService.CreateLabelForCustomer(louise.CustomerId, 55, 350m);
                barcodeService.CreateLabelForCustomer(louise.CustomerId, 55, 125m);
            }

            // Opret nogle labels for Sofie's reoler
            var sofie = customerRepo.GetByPhone("55443322");
            if (sofie != null)
            {
                barcodeService.CreateLabelForCustomer(sofie.CustomerId, 12, 50m);
                barcodeService.CreateLabelForCustomer(sofie.CustomerId, 13, 100m);
                barcodeService.CreateLabelForCustomer(sofie.CustomerId, 14, 150m);
            }
        }

        /// <summary>
        /// Henter statistik for test data
        /// </summary>
        public static string GetTestDataSummary()
        {
            var rackRepo = ServiceLocator.RackRepository;
            var customerRepo = ServiceLocator.CustomerRepository;
            var rentalService = ServiceLocator.RentalService;

            var totalRacks = rackRepo.GetAll().Count();
            var availableRacks = rackRepo.GetByAvailability(true).Count();
            var occupiedRacks = rackRepo.GetByAvailability(false).Count();
            var totalCustomers = customerRepo.GetAll().Count();
            var activeAgreements = rentalService.GetAllActiveAgreements().Count();

            return $"Test Data Summary:\n" +
                   $"- Reoler total: {totalRacks}\n" +
                   $"- Ledige reoler: {availableRacks}\n" +
                   $"- Udlejede reoler: {occupiedRacks}\n" +
                   $"- Kunder: {totalCustomers}\n" +
                   $"- Aktive lejeaftaler: {activeAgreements}";
        }
    }
}