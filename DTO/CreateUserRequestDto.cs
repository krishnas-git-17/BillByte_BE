using BillByte.Models;

namespace BillByte.DTO
{
    public class CreateUserRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
