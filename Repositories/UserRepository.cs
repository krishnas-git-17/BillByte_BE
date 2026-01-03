using BillByte.Models;
using BillByte.Repositories.Interface;
using Billbyte_BE.Data;

namespace BillByte.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        /* ===== LOGIN ===== */

        public User? GetByEmail(string email)
        {
            return _context.Users
                .FirstOrDefault(x =>
                    x.Email == email &&
                    x.IsActive);
        }

        public User? GetByEmployeeId(string employeeId)
        {
            return _context.Users
                .FirstOrDefault(x =>
                    x.EmployeeId == employeeId &&
                    x.IsActive);
        }

        /* ===== EMPLOYEE ID SEQUENCE ===== */

        public int GetNextEmployeeSequence(int restaurantId, UserRole role)
        {
            return _context.Users
                .Where(x =>
                    x.RestaurantId == restaurantId &&
                    x.Role == role)
                .Count() + 1;
        }

        /* ===== USERS ===== */

        public IEnumerable<User> GetByRestaurant(int restaurantId)
        {
            return _context.Users
                .Where(x => x.RestaurantId == restaurantId)
                .OrderBy(x => x.Role)
                .ThenBy(x => x.EmployeeId)
                .ToList();
        }

        public User? GetById(int id)
        {
            return _context.Users
                .FirstOrDefault(x => x.Id == id);
        }

        /* ===== CRUD ===== */

        public void Add(User user)
        {
            _context.Users.Add(user);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
