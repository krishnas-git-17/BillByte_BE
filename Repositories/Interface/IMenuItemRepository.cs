using BillByte.Model;

namespace BillByte.Interface
{
    public interface IMenuItemRepository
    {
        Task<List<MenuItem>> GetAllAsync();
        Task<MenuItem?> GetByIdAsync(int id);
        Task<MenuItem> AddAsync(MenuItem item);
        Task<MenuItem> UpdateAsync(MenuItem item);
        Task<bool> DeleteAsync(int id);
        Task<List<MenuItem>> GetByFoodTypeAsync(int typeId);
    }
}
