namespace Billbyte_BE.Models
{
    public class TableState
    {
      
            public int Id { get; set; }
            public string TableId { get; set; } = null!;

            public string Status { get; set; } = "available";

            public DateTime? StartTime { get; set; }

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        

    }
}
