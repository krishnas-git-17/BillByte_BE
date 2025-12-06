using System.ComponentModel.DataAnnotations;

namespace BillByte.Model
{
    public class BusinessUnitSetting
    {
        [Key]
        public int Id { get; set; }

        public bool IsTableServeNeeded { get; set; }
        public int NonAcTables { get; set; }
        public int AcTables { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
