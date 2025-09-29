using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Services;

namespace ReolMarked.MVVM.Infrastructure
{
    /// <summary>
    /// Service Locator pattern - deler samme instans af alle services
    /// Dette sikrer at alle ViewModels arbejder med samme data
    /// </summary>
    public static class ServiceLocator
    {
        private static RackRepository _rackRepository;
        private static CustomerRepository _customerRepository;
        private static RentalService _rentalService;
        private static BarcodeService _barcodeService;
        private static SaleService _saleService;
        private static TerminationService _terminationService;

        public static RackRepository RackRepository
        {
            get
            {
                if (_rackRepository == null)
                {
                    _rackRepository = new RackRepository();
                }
                return _rackRepository;
            }
        }

        public static CustomerRepository CustomerRepository
        {
            get
            {
                if (_customerRepository == null)
                {
                    _customerRepository = new CustomerRepository();
                }
                return _customerRepository;
            }
        }

        public static RentalService RentalService
        {
            get
            {
                if (_rentalService == null)
                {
                    _rentalService = new RentalService(CustomerRepository, RackRepository);
                }
                return _rentalService;
            }
        }

        public static BarcodeService BarcodeService
        {
            get
            {
                if (_barcodeService == null)
                {
                    _barcodeService = new BarcodeService(CustomerRepository, RackRepository, RentalService);
                }
                return _barcodeService;
            }
        }

        public static SaleService SaleService
        {
            get
            {
                if (_saleService == null)
                {
                    _saleService = new SaleService(BarcodeService);
                }
                return _saleService;
            }
        }

        public static TerminationService TerminationService
        {
            get
            {
                if (_terminationService == null)
                {
                    _terminationService = new TerminationService(CustomerRepository, RackRepository, RentalService);
                }
                return _terminationService;
            }
        }

        /// <summary>
        /// Nulstil alle services (til test eller genstart)
        /// </summary>
        public static void Reset()
        {
            _rackRepository = null;
            _customerRepository = null;
            _rentalService = null;
            _barcodeService = null;
            _saleService = null;
            _terminationService = null;
        }
    }
}