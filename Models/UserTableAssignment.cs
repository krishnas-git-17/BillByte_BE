using BillByte.Models;

namespace Billbyte_BE.Models
{
    public class UserTableAssignment
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public int UserId { get; set; }               // Users.Id (INT)
        public int TablePreferenceId { get; set; }    // Section Id

        public User? User { get; set; }
        public TablePreference? TablePreference { get; set; }
    }
}
