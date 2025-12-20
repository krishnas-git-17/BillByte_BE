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

        public async Task<List<TableState>> GetAllAsync()
        {
            return await _context.TableStates.ToListAsync();
        }

        public async Task<TableState?> GetByTableIdAsync(string tableId)
        {
            return await _context.TableStates
                .FirstOrDefaultAsync(x => x.TableId == tableId);
        }

        public async Task StartTimerAsync(string tableId)
        {
            var state = await GetByTableIdAsync(tableId);

            if (state == null)
            {
                state = new TableState
                {
                    TableId = tableId,
                    Status = "occupied",
                    StartTime = DateTime.UtcNow
                };
                _context.TableStates.Add(state);
            }
            else
            {
                state.Status = "occupied";
                state.StartTime = DateTime.UtcNow;
                state.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task StopTimerAsync(string tableId)
        {
            var state = await GetByTableIdAsync(tableId);
            if (state == null) return;

            // 🔥 USE DATA HERE IF REQUIRED (elapsed time)
            // var elapsedMinutes = (DateTime.UtcNow - state.StartTime)?.TotalMinutes;

            _context.TableStates.Remove(state); // 🔥 DELETE IMMEDIATELY
            await _context.SaveChangesAsync();
        }

        public async Task SetStatusAsync(string tableId, string status)
        {
            var state = await GetByTableIdAsync(tableId);

            if (state == null)
            {
                state = new TableState
                {
                    TableId = tableId,
                    Status = status
                };
                _context.TableStates.Add(state);
            }
            else
            {
                state.Status = status;
                state.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TableState state)
        {
            _context.TableStates.Remove(state);
            await _context.SaveChangesAsync();
        }
    }

}
