namespace BillByte.Model
{
    public class MenuItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int FoodTypeId { get; set; }
        public decimal ItemCost { get; set; }
        public decimal? GSTPercentage { get; set; }
        public decimal? CGSTPercentage { get; set; }
        public string? ImageUrl { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
