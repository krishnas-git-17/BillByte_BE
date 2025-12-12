namespace BillByte.Model
{
    public class MenuItemImgs
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemImage { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
