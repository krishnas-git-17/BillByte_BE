using Billbyte_BE.Models;

public interface IActiveTableItemRepository
{
    Task<List<ActiveTableItem>> GetByTableAsync(string tableId, int restaurantId);

    Task AddOrUpdateAsync(ActiveTableItem item);

    Task UpdateQtyAsync(string tableId, int itemId, int qty, int restaurantId);

    Task DeleteItemAsync(string tableId, int itemId, int restaurantId);

    Task ClearTableAsync(string tableId, int restaurantId);
}
