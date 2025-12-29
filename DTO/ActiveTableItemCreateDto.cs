namespace Billbyte_BE.DTO
{
    public class ActiveTableItemCreateDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public decimal Price { get; set; }
        public int Qty { get; set; }
    }
}
