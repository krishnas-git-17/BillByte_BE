using BillByte.Model;

namespace Billbyte_BE.Repositories.Interface
{
    public interface ICompletedOrderRepository
    {
        Task AddOrderAsync(CompletedOrder order);
        Task<List<CompletedOrder>> GetTodayOrdersAsync();
        Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(DateTime from, DateTime to);
    }
}
