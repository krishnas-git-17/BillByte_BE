using BillByte.Models;

namespace BillByte.Repositories.Interface
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
    }
}
