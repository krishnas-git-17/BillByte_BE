namespace BillByte.DTO
{
    public class AssignTablesRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty; 
        public List<int> TablePreferenceIds { get; set; } = new();
    }
}
