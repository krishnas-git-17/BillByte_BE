using BillByte.Model;

namespace Billbyte_BE.Repositories.Interface
{
    public interface ICompletedOrderRepository
    {
        Task AddOrderAsync(CompletedOrder order);
        Task<List<CompletedOrder>> GetAllAsync(int restaurantId);
        Task<List<CompletedOrder>> GetOrdersByDateRangeAsync(
            int restaurantId,
            DateTime from,
            DateTime to);
    }
}
