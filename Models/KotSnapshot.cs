namespace BillByte.Model
{
    public class KotSnapshot
    {
        public int Id { get; set; }
        public int RestaurantId { get; set; }
        public string TableId { get; set; } = string.Empty;

        public DateTime BusinessDate { get; set; }

        public int KotNo { get; set; }              // daily sequence number
        public string KotNumber { get; set; } = ""; // KOT-RESTID-YYYYMMDD-XXX

        public DateTime CreatedAt { get; set; }     // ✅ FIXED

        public List<KotSnapshotItem> Items { get; set; } = new();
    }
}
