using BillByte.Models;

namespace BillByte.Repositories.Interface
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);

        // ✅ single, correct method
        User? GetByEmployeeId(string employeeId);

        int GetNextEmployeeSequence(int restaurantId, UserRole role);

        IEnumerable<User> GetByRestaurant(int restaurantId);
        User? GetById(int id);

        void Add(User user);
        void Save();
    }
}
