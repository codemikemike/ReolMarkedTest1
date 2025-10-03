// Services/BarcodeService.cs
using ReolMarked.MVVM.Models;
using ReolMarked.MVVM.Repositories.Interfaces;
using ReolMarked.MVVM.Services.DTOs;
using ReolMarked.MVVM.Services.Results;
using System.Text;

namespace ReolMarked.MVVM.Services
{
    /// <summary>
    /// Service til at håndtere stregkode generering
    /// AL forretningslogik for labels
    /// </summary>
    public class BarcodeService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IRackRepository _rackRepository;
        private readonly RentalService _rentalService;

        public BarcodeService(
            ILabelRepository labelRepository,
            ICustomerRepository customerRepository,
            IRackRepository rackRepository,
            RentalService rentalService)
        {
            _labelRepository = labelRepository;
            _customerRepository = customerRepository;
            _rackRepository = rackRepository;
            _rentalService = rentalService;
        }

        /// <summary>
        /// Validerer at en kunde ejer den angivne reol
        /// </summary>
        public bool ValidateCustomerOwnsRack(int customerId, int rackNumber)
        {
            var customerRacks = _rentalService.GetRacksForCustomer(customerId);
            return customerRacks.Any(r => r.RackNumber == rackNumber);
        }

        /// <summary>
        /// Genererer stregkode baseret på reol og pris
        /// Format: "REOL07-50KR-001"
        /// </summary>
        public string GenerateBarcode(int rackNumber, decimal price, int labelId)
        {
            int priceAsInt = (int)price;
            return $"REOL{rackNumber:D2}-{priceAsInt}KR-{labelId:D3}";
        }

        /// <summary>
        /// Parser en stregkode og returnerer reolnummer og pris
        /// </summary>
        public bool ParseBarcode(string barcode, out int rackNumber, out decimal price)
        {
            rackNumber = 0;
            price = 0;

            if (string.IsNullOrEmpty(barcode))
                return false;

            try
            {
                // Format: "REOL07-50KR-001"
                string[] parts = barcode.Split('-');
                if (parts.Length >= 2)
                {
                    // Parse reol nummer
                    string rackPart = parts[0];
                    if (rackPart.StartsWith("REOL"))
                    {
                        string rackNumberText = rackPart.Substring(4);
                        if (int.TryParse(rackNumberText, out rackNumber))
                        {
                            // Parse pris
                            string pricePart = parts[1];
                            if (pricePart.EndsWith("KR"))
                            {
                                string priceText = pricePart.Substring(0, pricePart.Length - 2);
                                if (decimal.TryParse(priceText, out price))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignorer parsing fejl
            }

            return false;
        }

        /// <summary>
        /// Opretter et enkelt label for en kunde
        /// </summary>
        public Label CreateLabelForCustomer(int customerId, int rackNumber, decimal price)
        {
            // Valider at kunden ejer reolen
            if (!ValidateCustomerOwnsRack(customerId, rackNumber))
                return null;

            var customer = _customerRepository.GetById(customerId);
            var rack = _rackRepository.GetByRackNumber(rackNumber);

            if (customer == null || rack == null)
                return null;

            // Opret nyt label
            var label = new Label
            {
                RackId = rackNumber,
                ProductPrice = price,
                IsVoid = false
            };

            // Gem i repository (får auto-genereret ID)
            label = _labelRepository.Add(label);

            // Generer stregkode EFTER vi har ID
            label.BarCode = GenerateBarcode(rackNumber, price, label.LabelId);
            _labelRepository.Update(label);

            // Fyld navigation properties
            label.Customer = customer;
            label.Rack = rack;

            return label;
        }

        /// <summary>
        /// Opretter flere labels for forskellige produkter
        /// </summary>
        public BarcodeGenerationResult GenerateLabelsForProducts(
            int customerId,
            int rackNumber,
            List<LabelRequest> products)
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
                    var label = CreateLabelForCustomer(customerId, rackNumber, product.Price);
                    if (label != null)
                    {
                        createdLabels.Add(label);
                    }
                }
            }

            result.Success = true;
            result.CreatedLabels = createdLabels;
            result.PrintOutput = GeneratePrintOutput(createdLabels);

            return result;
        }

        /// <summary>
        /// Genererer print output til stregkoder
        /// </summary>
        public string GeneratePrintOutput(List<Label> labels)
        {
            var output = new StringBuilder();
            output.AppendLine("=== STREGKODE PRINT OUTPUT ===");
            output.AppendLine($"Printet: {DateTime.Now:dd/MM/yyyy HH:mm}");
            output.AppendLine("==============================");
            output.AppendLine();

            foreach (var label in labels)
            {
                const int boxWidth = 37;
                string topLine = "+" + new string('-', boxWidth - 2) + "+";
                string emptyLine = "|" + new string(' ', boxWidth - 2) + "|";

                string headerContent = "  MIDDELBY REOLMARKED";
                string headerLine = "|" + headerContent + new string(' ', boxWidth - 2 - headerContent.Length) + "|";

                string barcodeContent = $"  {label.BarCode}";
                string barcodeLine = "|" + barcodeContent + new string(' ', boxWidth - 2 - barcodeContent.Length) + "|";

                string reolPrisContent = $"  Reol: {label.RackId}    Pris: {label.ProductPrice:F0} kr.";
                string reolPrisLine = "|" + reolPrisContent + new string(' ', boxWidth - 2 - reolPrisContent.Length) + "|";

                string dateContent = $"  {label.CreatedAt:dd/MM/yyyy HH:mm}";
                string dateLine = "|" + dateContent + new string(' ', boxWidth - 2 - dateContent.Length) + "|";

                output.AppendLine(topLine);
                output.AppendLine(headerLine);
                output.AppendLine(emptyLine);
                output.AppendLine(barcodeLine);
                output.AppendLine(emptyLine);
                output.AppendLine(reolPrisLine);
                output.AppendLine(dateLine);
                output.AppendLine(topLine);
                output.AppendLine();
            }

            output.AppendLine($"Total antal labels: {labels.Count}");
            output.AppendLine("==============================");

            return output.ToString();
        }

        /// <summary>
        /// Markerer et label som solgt
        /// </summary>
        public void MarkLabelAsSold(int labelId)
        {
            var label = _labelRepository.GetById(labelId);
            if (label != null && !label.IsVoid)
            {
                label.SoldDate = DateTime.Now;  // Ændret fra label.Sold
                _labelRepository.Update(label);
            }
        }

        /// <summary>
        /// Annullerer et label
        /// </summary>
        public bool VoidLabel(int labelId)
        {
            var label = _labelRepository.GetById(labelId);
            if (label == null)
                return false;

            label.IsVoid = true;
            _labelRepository.Update(label);
            return true;
        }

        /// <summary>
        /// Finder et label baseret på stregkode
        /// </summary>
        public Label FindLabelByBarcode(string barcode)
        {
            return _labelRepository.GetByBarcode(barcode);
        }

        /// <summary>
        /// Henter alle labels for en kunde
        /// </summary>
        public IEnumerable<Label> GetLabelsForCustomer(int customerId)
        {
            return _labelRepository.GetByCustomerId(customerId)
                .Where(l => !l.IsVoid);
        }

        /// <summary>
        /// Henter alle aktive labels
        /// </summary>
        public IEnumerable<Label> GetAllActiveLabels()
        {
            return _labelRepository.GetActiveLabels();
        }
    }
}