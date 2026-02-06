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

        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public bool ForcePasswordChange { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; } = false;
        public string? EmailOtp { get; set; }

        public int? PlanId { get; set; }
        public bool IsPlanActive { get; set; } = false;
        public DateTime? PlanExpiryDate { get; set; }


        public DateTime? EmailOtpExpiry { get; set; }

        public int? CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}
