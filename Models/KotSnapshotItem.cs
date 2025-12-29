namespace BillByte.Model
{
    public class KotSnapshotItem
    {
        public int Id { get; set; }
        public int KotSnapshotId { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public string? SpecialNote { get; set; }
    }
}
