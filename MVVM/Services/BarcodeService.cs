using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service klasse til at håndtere stregkode generering og print (UC3.2)
    /// Håndterer forretningslogikken for at oprette labels til produkter
    /// </summary>
    public class BarcodeService
    {
        // Private liste til at gemme alle labels
        private List<Label> _labels;
        private int _nextLabelId; // Til at tildele unikke ID'er

        // Reference til andre services og repositories
        private CustomerRepository _customerRepo;
        private RackRepository _rackRepo;
        private RentalService _rentalService;

        // Konstruktør - opretter service
        public BarcodeService(CustomerRepository customerRepo, RackRepository rackRepo, RentalService rentalService)
        {
            _labels = new List<Label>();
            _nextLabelId = 1;
            _customerRepo = customerRepo;
            _rackRepo = rackRepo;
            _rentalService = rentalService;
            CreateTestData();
        }

        /// <summary>
        /// Opretter nogle test labels
        /// </summary>
        private void CreateTestData()
        {
            // Peter har nogle eksisterende labels på sine reoler
            var peter = _customerRepo.GetCustomerByPhone("12345678");
            if (peter != null)
            {
                CreateLabelForCustomer(peter.CustomerId, 7, 45.00m, "Vintage Bog");
                CreateLabelForCustomer(peter.CustomerId, 42, 125.00m, "Keramikskål");
            }
        }

        /// <summary>
        /// Validerer at en kunde ejer den angivne reol
        /// </summary>
        public bool ValidateCustomerOwnsRack(int customerId, int rackNumber)
        {
            // Hent kundens reoler fra RentalService
            var customerRacks = _rentalService.GetRacksForCustomer(customerId);

            // Tjek om kunden har den angivne reol
            foreach (var rack in customerRacks)
            {
                if (rack.RackNumber == rackNumber)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Finder kunde baseret på telefonnummer eller navn
        /// </summary>
        public Customer FindCustomerByPhone(string phoneNumber)
        {
            return _customerRepo.GetCustomerByPhone(phoneNumber);
        }

        /// <summary>
        /// Opretter et enkelt label for en kunde
        /// </summary>
        public Label CreateLabelForCustomer(int customerId, int rackNumber, decimal price, string productName = "")
        {
            // Valider at kunden ejer reolen
            if (!ValidateCustomerOwnsRack(customerId, rackNumber))
            {
                return null; // Kunde ejer ikke reolen
            }

            // Opret nyt label
            var newLabel = new Label
            {
                LabelId = _nextLabelId++,
                RackId = rackNumber,
                ProductPrice = price,
                CreatedAt = DateTime.Now
            };

            // Generer stregkode automatisk
            newLabel.GenerateBarCode();

            // Find kunde og reol for navigation properties
            newLabel.Customer = _customerRepo.GetCustomerById(customerId);
            newLabel.Rack = _rackRepo.GetRackByNumber(rackNumber);

            // Tilføj til listen
            _labels.Add(newLabel);

            return newLabel;
        }

        /// <summary>
        /// Opretter flere labels for samme produkt (UC3.2 bulk creation)
        /// </summary>
        public ObservableCollection<Label> CreateLabelsForCustomer(int customerId, int rackNumber, decimal price, int quantity, string productName = "")
        {
            var createdLabels = new List<Label>();

            // Valider at kunden ejer reolen
            if (!ValidateCustomerOwnsRack(customerId, rackNumber))
            {
                return new ObservableCollection<Label>(createdLabels); // Tom liste hvis fejl
            }

            // Opret det ønskede antal labels
            for (int i = 0; i < quantity; i++)
            {
                var label = CreateLabelForCustomer(customerId, rackNumber, price, productName);
                if (label != null)
                {
                    createdLabels.Add(label);
                }
            }

            return new ObservableCollection<Label>(createdLabels);
        }

        /// <summary>
        /// Opretter labels for en liste af forskellige produkter (UC3.2 main scenario)
        /// </summary>
        public BarcodeGenerationResult GenerateLabelsForProducts(int customerId, int rackNumber, List<LabelRequest> products)
        {
            var result = new BarcodeGenerationResult();

            // Valider at kunden ejer reolen
            if (!ValidateCustomerOwnsRack(customerId, rackNumber))
            {
                result.Success = false;
                result.ErrorMessage = "Fejl: Reolen er ikke udlejet til dig eller eksisterer ikke.";
                return result;
            }

            var createdLabels = new List<Label>();

            // Opret labels for hvert produkt
            foreach (var product in products)
            {
                for (int i = 0; i < product.Quantity; i++)
                {
                    var label = CreateLabelForCustomer(customerId, rackNumber, product.Price, product.Name);
                    if (label != null)
                    {
                        createdLabels.Add(label);
                    }
                }
            }

            result.Success = true;
            result.CreatedLabels = new ObservableCollection<Label>(createdLabels);
            result.PrintOutput = GeneratePrintOutput(createdLabels);

            return result;
        }

        /// <summary>
        /// Genererer print output til stregkoder (simulerer printer)
        /// </summary>
        public string GeneratePrintOutput(List<Label> labels)
        {
            var printOutput = new StringBuilder();
            printOutput.AppendLine("=== STREGKODE PRINT OUTPUT ===");
            printOutput.AppendLine($"Printet: {DateTime.Now:dd/MM/yyyy HH:mm}");
            printOutput.AppendLine("==============================");
            printOutput.AppendLine();

            foreach (var label in labels)
            {
                printOutput.AppendLine($"┌─────────────────────────────┐");
                printOutput.AppendLine($"│  MIDDELBY REOLMARKED        │");
                printOutput.AppendLine($"│                             │");
                printOutput.AppendLine($"│  {label.BarCode,-25}  │");
                printOutput.AppendLine($"│                             │");
                printOutput.AppendLine($"│  Reol: {label.RackId,-2}    Pris: {label.ProductPrice:F0} kr. │");
                printOutput.AppendLine($"│  {label.CreatedAt:dd/MM/yyyy HH:mm}               │");
                printOutput.AppendLine($"└─────────────────────────────┘");
                printOutput.AppendLine();
            }

            printOutput.AppendLine($"Total antal labels: {labels.Count}");
            printOutput.AppendLine("==============================");

            return printOutput.ToString();
        }

        /// <summary>
        /// Henter alle labels for en kunde
        /// </summary>
        public ObservableCollection<Label> GetLabelsForCustomer(int customerId)
        {
            var customerLabels = new List<Label>();

            foreach (var label in _labels)
            {
                if (label.Customer?.CustomerId == customerId && !label.IsVoid)
                {
                    customerLabels.Add(label);
                }
            }

            return new ObservableCollection<Label>(customerLabels);
        }

        /// <summary>
        /// Henter alle labels for en specifik reol
        /// </summary>
        public ObservableCollection<Label> GetLabelsForRack(int rackNumber)
        {
            var rackLabels = new List<Label>();

            foreach (var label in _labels)
            {
                if (label.RackId == rackNumber && !label.IsVoid)
                {
                    rackLabels.Add(label);
                }
            }

            return new ObservableCollection<Label>(rackLabels);
        }

        /// <summary>
        /// Annullerer et label
        /// </summary>
        public bool VoidLabel(int labelId)
        {
            foreach (var label in _labels)
            {
                if (label.LabelId == labelId)
                {
                    label.VoidLabel(labelId);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Henter alle aktive labels
        /// </summary>
        public ObservableCollection<Label> GetAllActiveLabels()
        {
            var activeLabels = new List<Label>();

            foreach (var label in _labels)
            {
                if (!label.IsVoid && !label.IsSold)
                {
                    activeLabels.Add(label);
                }
            }

            return new ObservableCollection<Label>(activeLabels);
        }

        /// <summary>
        /// Finder et label baseret på stregkode
        /// </summary>
        public Label FindLabelByBarcode(string barcode)
        {
            foreach (var label in _labels)
            {
                if (label.BarCode == barcode && !label.IsVoid)
                {
                    return label;
                }
            }

            return null;
        }

        /// <summary>
        /// Tæller antal labels for en kunde
        /// </summary>
        public int CountLabelsForCustomer(int customerId)
        {
            int count = 0;

            foreach (var label in _labels)
            {
                if (label.Customer?.CustomerId == customerId && !label.IsVoid)
                {
                    count++;
                }
            }

            return count;
        }
    }
}