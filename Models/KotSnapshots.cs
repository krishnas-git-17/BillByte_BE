namespace Billbyte_BE.Models
{
    public class KotSnapshot
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string TableId { get; set; } = null!;

        public int ItemId { get; set; }

        public int Qty { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
