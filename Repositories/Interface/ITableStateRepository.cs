using Billbyte_BE.Models;

namespace Billbyte_BE.Repositories.Interface
{
    public interface ITableStateRepository
    {
        Task<TableState?> GetByTableIdAsync(string tableId);
        Task<List<TableState>> GetAllAsync();

        Task StartTimerAsync(string tableId);
        Task StopTimerAsync(string tableId);

        Task SetStatusAsync(string tableId, string status);

        Task DeleteAsync(TableState state);
    }

}
