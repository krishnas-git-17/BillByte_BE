using BillByte.Hubs;
using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class ActiveTableItemRepository : IActiveTableItemRepository
{
    private readonly AppDbContext _context;
    private readonly IHubContext<PosHub> _hub;

    public ActiveTableItemRepository(
        AppDbContext context,
        IHubContext<PosHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    // 🔔 Broadcast helper (SAME STYLE as TableStateRepository)
    private async Task BroadcastAsync(string tableId, int restaurantId)
    {
        var items = await _context.ActiveTableItems
            .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
            .Select(x => new
            {
                itemId = x.ItemId,
                itemName = x.ItemName,
                qty = x.Qty,
                price = x.Price
            })
            .ToListAsync();

        await _hub.Clients
            .Group(restaurantId.ToString())
            .SendAsync("ACTIVE_TABLE_ITEMS_CHANGED", new
            {
                tableId,
                items
            });

        Console.WriteLine(
          $"SignalR: Active items updated → {System.Text.Json.JsonSerializer.Serialize(items)}"
        );
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

        // 🔔 SignalR
        await BroadcastAsync(item.TableId, item.RestaurantId);
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

        // 🔔 SignalR
        await BroadcastAsync(tableId, restaurantId);
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

        // 🔔 SignalR
        await BroadcastAsync(tableId, restaurantId);
    }

    public async Task ClearTableAsync(string tableId, int restaurantId)
    {
        var items = await _context.ActiveTableItems
            .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
            .ToListAsync();

        _context.ActiveTableItems.RemoveRange(items);
        await _context.SaveChangesAsync();

        // 🔔 SignalR (empty items)
        await _hub.Clients
            .Group(restaurantId.ToString())
            .SendAsync("ACTIVE_TABLE_ITEMS_CHANGED", new
            {
                tableId,
                items = Array.Empty<object>()
            });
    }
}
