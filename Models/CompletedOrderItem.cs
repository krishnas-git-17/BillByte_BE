namespace BillByte.Model
{
    public class CompletedOrderItem
    {
        public int Id { get; set; }

        public int CompletedOrderId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Qty { get; set; }
    }
}
