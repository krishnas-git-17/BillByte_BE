using BillByte.Model;
using Billbyte_BE.Models;

namespace BillByte.Interface
{
    public interface ITablePreferenceRepository
    {
        Task<List<TablePreference>> GetAllAsync();
        Task<TablePreference?> GetByIdAsync(int id);
        Task AddAsync(TablePreference item);
        Task AddRangeAsync(List<TablePreference> items);
        Task UpdateAsync(TablePreference item);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteAllAsync();
    }
}
