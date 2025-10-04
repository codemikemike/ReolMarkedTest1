// Services/DTOs/LabelRequest.cs
namespace ReolMarked.MVVM.Services.DTOs
{
    //Data Transfer Object - only properties needed to create a label, no logic or methods
    //For isolation from the database model
    public class LabelRequest
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}