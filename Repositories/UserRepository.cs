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

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(x => x.Email == email && x.IsActive);
        }
    }
}
