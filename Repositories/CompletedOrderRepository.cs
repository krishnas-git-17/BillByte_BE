using BillByte.Model;
using Billbyte_BE.Data;
using Billbyte_BE.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Repositories
{
    public class CompletedOrderRepository : ICompletedOrderRepository
    {
        private readonly AppDbContext _context;

        public CompletedOrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(CompletedOrder order)
        {
            _context.CompletedOrders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CompletedOrder>> GetTodayOrdersAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _context.CompletedOrders
                .Include(o => o.Items)
                .Where(o => o.CreatedDate.Date == today)
                .ToListAsync();
        }

        public async Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _context.CompletedOrders
                .Include(o => o.Items)
                .Where(o => o.CreatedDate >= from && o.CreatedDate <= to)
                .ToListAsync();
        }
    }
}
