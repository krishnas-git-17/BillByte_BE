namespace Billbyte_BE.DTO
{
    public class KotItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int QtyChange { get; set; }   // + or -
    }
}
