namespace BillByte.Models
{
    public enum UserRole
    {
        Owner = 1,
        Admin = 2,
        Cashier = 3,
        Waiter = 4
    }

    public class User
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        // Staff identity
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Login
        public string? Email { get; set; }   // nullable for staff
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        // Security
        public bool ForcePasswordChange { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Audit
        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
