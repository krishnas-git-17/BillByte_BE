using BillByte.Model;

namespace BillByte.Interface
{
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> GetAllAsync(int restaurantId);
        Task<MenuItem?> GetByIdAsync(string id, int restaurantId);
        Task<MenuItem> AddAsync(MenuItem item);
        Task<MenuItem> UpdateAsync(MenuItem item);
        Task<bool> DeleteAsync(string id, int restaurantId);
    }
}
