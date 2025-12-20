using BillByte.Model;

namespace Billbyte_BE.Repositories.Interface
{
    public interface ICompletedOrderRepository
    {
        Task AddOrderAsync(CompletedOrder order);
        Task<List<CompletedOrder>> GetAllAsync();
        Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(DateTime from, DateTime to);
    }
}
