namespace BillByte.Model
{
    public class MenuItem
    {
        public string MenuId { get; set; } = Guid.NewGuid().ToString();
        public int RestaurantId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string VegType { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
    }
}
