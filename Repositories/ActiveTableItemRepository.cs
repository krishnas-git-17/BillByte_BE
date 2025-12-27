using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Microsoft.EntityFrameworkCore;

public class ActiveTableItemRepository : IActiveTableItemRepository
{
    private readonly AppDbContext _context;

    public ActiveTableItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActiveTableItem>> GetByTableAsync(string tableId, int restaurantId)
    {
        return await _context.ActiveTableItems
            .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
            .ToListAsync();
    }

    public async Task AddOrUpdateAsync(ActiveTableItem item)
    {
        var existing = await _context.ActiveTableItems.FirstOrDefaultAsync(x =>
            x.TableId == item.TableId &&
            x.ItemId == item.ItemId &&
            x.RestaurantId == item.RestaurantId);

        if (existing == null)
        {
            _context.ActiveTableItems.Add(item);
        }
        else
        {
            existing.Qty += item.Qty;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateQtyAsync(string tableId, int itemId, int qty, int restaurantId)
    {
        var item = await _context.ActiveTableItems.FirstOrDefaultAsync(x =>
            x.TableId == tableId &&
            x.ItemId == itemId &&
            x.RestaurantId == restaurantId);

        if (item == null) return;

        if (qty <= 0)
            _context.ActiveTableItems.Remove(item);
        else
            item.Qty = qty;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteItemAsync(string tableId, int itemId, int restaurantId)
    {
        var item = await _context.ActiveTableItems.FirstOrDefaultAsync(x =>
            x.TableId == tableId &&
            x.ItemId == itemId &&
            x.RestaurantId == restaurantId);

        if (item == null) return;

        _context.ActiveTableItems.Remove(item);
        await _context.SaveChangesAsync();
    }

    public async Task ClearTableAsync(string tableId, int restaurantId)
    {
        var items = await _context.ActiveTableItems
            .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
            .ToListAsync();

        _context.ActiveTableItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }
}
