namespace BillByte.Models
{
    public class Plan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;   // FREE, BASIC, PRO
        public decimal Price { get; set; }
        public int MaxUsers { get; set; }
        public bool IsActive { get; set; } = true;
        public int DurationInDays { get; set; } // 30, 365, etc
    }
}
