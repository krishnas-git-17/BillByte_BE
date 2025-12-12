namespace BillByte.Model
{
    public class MenuItem
    {
        public string MenuId { get; set; }  
        public string Name { get; set; } 
        public string Type { get; set; }    
        public string VegType { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
    }
}
