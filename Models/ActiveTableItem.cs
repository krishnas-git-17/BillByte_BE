namespace Billbyte_BE.Models
{
    public class ActiveTableItem
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string TableId { get; set; } = null!;

        public int ItemId { get; set; }

        public string ItemName { get; set; } = null!;

        public decimal Price { get; set; }

        public int Qty { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
