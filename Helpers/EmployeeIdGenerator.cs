using BillByte.Models;

namespace BillByte.Helpers
{
    public static class EmployeeIdGenerator
    {
        public static string Generate(
            int restaurantId,
            UserRole role,
            int nextSequence
        )
        {
            var prefix = role switch
            {
                UserRole.Owner => "OWN",
                UserRole.Admin => "ADM",
                UserRole.Cashier => "CAS",
                UserRole.Waiter => "WTR",
                _ => "EMP"
            };

            // Example: BB-1-WTR-0007
            return $"BB-{restaurantId}-{prefix}-{nextSequence:D4}";
        }
    }
}
