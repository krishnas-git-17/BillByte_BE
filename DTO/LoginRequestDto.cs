namespace BillByte.DTO
{
    public class LoginRequestDto
    {
        // Either Email or EmployeeId
        public string? Email { get; set; }
        public string? EmployeeId { get; set; }

        public string Password { get; set; } = string.Empty;
    }
}
