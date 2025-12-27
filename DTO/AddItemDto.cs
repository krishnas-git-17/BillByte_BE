public class AddItemDto
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Qty { get; set; }
}
