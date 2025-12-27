namespace BillByte.Model
{
    public class CompletedOrder
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string TableId { get; set; } = string.Empty;

        public string OrderType { get; set; } = string.Empty;

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        public int TableTimeMinutes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public List<CompletedOrderItem> Items { get; set; } = new();
    }
}
