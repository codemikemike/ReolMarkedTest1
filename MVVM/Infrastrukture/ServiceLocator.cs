using ReolMarked.MVVM.Repositories;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services;

namespace ReolMarked.MVVM.Infrastructure
{
    public static class ServiceLocator
    {
        // Repositories
        private static IRackRepository _rackRepository;
        private static ICustomerRepository _customerRepository;
        private static IRentalAgreementRepository _rentalAgreementRepository;
        private static ILabelRepository _labelRepository;
        private static ISaleRepository _saleRepository;
        private static ISaleLineRepository _saleLineRepository;
        private static IRackSaleRepository _rackSaleRepository;
        private static IRackTerminationRepository _rackTerminationRepository;
        private static IInvoiceRepository _invoiceRepository;

        // Services
        private static RentalService _rentalService;
        private static CustomerService _customerService;
        private static BarcodeService _barcodeService;
        private static SaleService _saleService;
        private static TerminationService _terminationService;
        private static InvoiceService _invoiceService;
        private static IWindowService _windowService;

        // Repository Properties
        public static IRackRepository RackRepository
        {
            get
            {
                if (_rackRepository == null)
                    _rackRepository = new RackRepository();
                return _rackRepository;
            }
        }

        public static ICustomerRepository CustomerRepository
        {
            get
            {
                if (_customerRepository == null)
                    _customerRepository = new CustomerRepository();
                return _customerRepository;
            }
        }

        public static IRentalAgreementRepository RentalAgreementRepository
        {
            get
            {
                if (_rentalAgreementRepository == null)
                    _rentalAgreementRepository = new RentalAgreementRepository();
                return _rentalAgreementRepository;
            }
        }

        public static ILabelRepository LabelRepository
        {
            get
            {
                if (_labelRepository == null)
                    _labelRepository = new LabelRepository();
                return _labelRepository;
            }
        }

        public static ISaleRepository SaleRepository
        {
            get
            {
                if (_saleRepository == null)
                    _saleRepository = new SaleRepository();
                return _saleRepository;
            }
        }

        public static ISaleLineRepository SaleLineRepository
        {
            get
            {
                if (_saleLineRepository == null)
                    _saleLineRepository = new SaleLineRepository();
                return _saleLineRepository;
            }
        }

        public static IRackSaleRepository RackSaleRepository
        {
            get
            {
                if (_rackSaleRepository == null)
                    _rackSaleRepository = new RackSaleRepository();
                return _rackSaleRepository;
            }
        }

        public static IRackTerminationRepository RackTerminationRepository
        {
            get
            {
                if (_rackTerminationRepository == null)
                    _rackTerminationRepository = new RackTerminationRepository();
                return _rackTerminationRepository;
            }
        }

        public static IInvoiceRepository InvoiceRepository
        {
            get
            {
                if (_invoiceRepository == null)
                    _invoiceRepository = new InvoiceRepository();
                return _invoiceRepository;
            }
        }

        // Service Properties
        public static RentalService RentalService
        {
            get
            {
                if (_rentalService == null)
                {
                    _rentalService = new RentalService(
                        RentalAgreementRepository,
                        RackRepository,
                        CustomerRepository);
                }
                return _rentalService;
            }
        }

        public static CustomerService CustomerService
        {
            get
            {
                if (_customerService == null)
                    _customerService = new CustomerService(CustomerRepository);
                return _customerService;
            }
        }

        public static BarcodeService BarcodeService
        {
            get
            {
                if (_barcodeService == null)
                {
                    _barcodeService = new BarcodeService(
                        LabelRepository,
                        CustomerRepository,
                        RackRepository,
                        RentalService);
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
                    _saleService = new SaleService(
                        SaleRepository,
                        SaleLineRepository,
                        RackSaleRepository,
                        BarcodeService);
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
                    _terminationService = new TerminationService(
                        RackTerminationRepository,
                        CustomerRepository,
                        RackRepository,
                        RentalService);
                }
                return _terminationService;
            }
        }

        public static InvoiceService InvoiceService
        {
            get
            {
                if (_invoiceService == null)
                {
                    _invoiceService = new InvoiceService(
                        InvoiceRepository,
                        CustomerRepository,
                        RackRepository,
                        RentalService,
                        SaleService);
                }
                return _invoiceService;
            }
        }

        public static IWindowService WindowService
        {
            get
            {
                if (_windowService == null)
                    _windowService = new WindowService();
                return _windowService;
            }
        }

        public static void Reset()
        {
            _rackRepository = null;
            _customerRepository = null;
            _rentalAgreementRepository = null;
            _labelRepository = null;
            _saleRepository = null;
            _saleLineRepository = null;
            _rackSaleRepository = null;
            _rackTerminationRepository = null;
            _invoiceRepository = null;

            _rentalService = null;
            _customerService = null;
            _barcodeService = null;
            _saleService = null;
            _terminationService = null;
            _invoiceService = null;
            _windowService = null;
        }
    }
}