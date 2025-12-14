namespace BillByte.Model
{
    public class CompletedOrder
    {
        public int Id { get; set; }

        public string TableId { get; set; } = string.Empty;

        public string OrderType { get; set; } = string.Empty; // Dine / Parcel / Delivery

        public decimal Subtotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Discount { get; set; }

        public decimal Total { get; set; }

        public string PaymentMode { get; set; } = string.Empty; // Cash / Card / UPI

        public int TableTimeMinutes { get; set; }   // time occupied

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // ✅ REQUIRED
        public List<CompletedOrderItem> Items { get; set; } = new();
    }
}
