// Services/DTOs/LabelRequest.cs
namespace ReolMarked.MVVM.Services.DTOs
{
    public class LabelRequest
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;
    }
}