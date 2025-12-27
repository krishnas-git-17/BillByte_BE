namespace Billbyte_BE.Models
{
    public class TablePreference
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TableCount { get; set; }
    }
}
