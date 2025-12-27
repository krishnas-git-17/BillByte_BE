using Billbyte_BE.Models;

namespace Billbyte_BE.Repositories.Interface
{
    public interface ITableStateRepository
    {
        Task<List<TableState>> GetAllAsync(int restaurantId);
        Task<TableState?> GetByTableIdAsync(string tableId, int restaurantId);

        Task SetOccupiedAsync(string tableId, int restaurantId);
        Task<bool> MoveToOrderedAsync(string tableId, int restaurantId);
        Task<bool> MoveToBillingAsync(string tableId, int restaurantId);
        Task ResetAsync(string tableId, int restaurantId);
    }
}
