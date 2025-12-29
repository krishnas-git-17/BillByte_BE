using Billbyte_BE.Models;

namespace BillByte.Interface
{
    public interface ITablePreferenceRepository
    {
        Task<List<TablePreference>> GetAllAsync(int restaurantId);
        Task<TablePreference?> GetByIdAsync(int id, int restaurantId);
        Task AddRangeAsync(List<TablePreference> items);
        Task UpdateAsync(TablePreference item);
        Task<bool> DeleteAsync(int id, int restaurantId);
        Task<bool> DeleteAllAsync(int restaurantId);
    }
}
