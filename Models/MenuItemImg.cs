namespace BillByte.Model
{
    public class MenuItemImgs
    {
        public int Id { get; set; }

        public string MenuId { get; set; } = string.Empty;
        public int RestaurantId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
    }
}
