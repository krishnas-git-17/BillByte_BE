using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using BillByte.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Billbyte_BE.Repositories
{
    public class TableStateRepository : ITableStateRepository
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<PosHub> _hub;

        public TableStateRepository(AppDbContext context, IHubContext<PosHub> hub)
        {
            _context = context;
            _hub = hub;
        }


        private async Task BroadcastTableStateAsync(TableState state)
        {
            await _hub.Clients
                .Group(state.RestaurantId.ToString())
                .SendAsync("TABLE_STATUS_CHANGED", new
                {
                    tableId = state.TableId,
                    status = state.Status,
                    startTime = state.StartTime
                });
            Console.WriteLine($"SignalR sent: {state.TableId} → {state.Status}");

        }


        public async Task<List<TableState>> GetAllAsync(int restaurantId)
        {
            return await _context.TableStates
                .Where(x => x.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<TableState?> GetByTableIdAsync(string tableId, int restaurantId)
        {
            return await _context.TableStates
                .FirstOrDefaultAsync(x =>
                    x.TableId == tableId &&
                    x.RestaurantId == restaurantId);
        }

        public async Task SetOccupiedAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);

            if (state == null)
            {
                // ✅ FIRST TIME ONLY
                state = new TableState
                {
                    TableId = tableId,
                    RestaurantId = restaurantId,
                    Status = "occupied",
                    StartTime = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.TableStates.Add(state);

            }
            else
            {
                // 🔒 DO NOT RESET TIMER
                if (state.Status == "occupied" || state.Status == "ordered")
                    return;

                state.Status = "occupied";

                if (state.StartTime == null)
                    state.StartTime = DateTime.UtcNow;

                state.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await BroadcastTableStateAsync(state);

        }



        public async Task<bool> MoveToOrderedAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);
            if (state == null)
                return false;

            // 🔒 Already ordered → do nothing
            if (state.Status == "ordered")
                return true;

            state.Status = "ordered";

            if (state.StartTime == null)
                state.StartTime = DateTime.UtcNow;

            state.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await BroadcastTableStateAsync(state);
            return true;

        }


        public async Task<bool> MoveToBillingAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);
            if (state == null || state.Status != "ordered")
                return false;

            state.Status = "billing";
            state.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await BroadcastTableStateAsync(state);
            return true;

        }

        public async Task<bool> MoveToReservationAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);

            if (state == null)
            {
                // First-time reservation
                state = new TableState
                {
                    TableId = tableId,
                    RestaurantId = restaurantId,
                    Status = "reservation",
                    StartTime = null,              // ❌ no timer
                    UpdatedAt = DateTime.UtcNow
                };

                _context.TableStates.Add(state);
            }
            else
            {
                // If already occupied or ordered → do not override
                if (state.Status == "occupied" || state.Status == "ordered")
                    return false;

                state.Status = "reservation";
                state.StartTime = null;           // ❌ ensure timer not started
                state.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            await BroadcastTableStateAsync(state);
            return true;

        }


        public async Task ResetAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);
            if (state == null) return;

            _context.TableStates.Remove(state);
            await _context.SaveChangesAsync();

            await _hub.Clients
                .Group(restaurantId.ToString())
                .SendAsync("TABLE_STATUS_CHANGED", new
                {
                    tableId,
                    status = "available",
                    startTime = (DateTime?)null
                });

        }
    }
}
