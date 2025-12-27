using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Repositories
{
    public class TableStateRepository : ITableStateRepository
    {
        private readonly AppDbContext _context;

        public TableStateRepository(AppDbContext context)
        {
            _context = context;
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
            return true;
        }

        public async Task ResetAsync(string tableId, int restaurantId)
        {
            var state = await GetByTableIdAsync(tableId, restaurantId);
            if (state == null) return;

            _context.TableStates.Remove(state);
            await _context.SaveChangesAsync();
        }
    }
}
