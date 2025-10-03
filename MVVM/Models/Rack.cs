namespace ReolMarked.MVVM.Models
{
    /// <summary>
    /// Ren POCO model for en reol
    /// </summary>
    public class Rack
    {
        public int RackId { get; set; }
        public int RackNumber { get; set; }
        public bool HasHangerBar { get; set; }
        public int AmountShelves { get; set; }
        public string Location { get; set; } = "";
        public bool IsAvailable { get; set; }
        public string Description { get; set; } = "";
    }
}