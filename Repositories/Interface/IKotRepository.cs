using BillByte.Model;

namespace Billbyte_BE.Repositories.Interface
{
    public interface IKotRepository
    {
        Task<KotSnapshot> CreateKotAsync(KotSnapshot kot);
        Task<List<KotSnapshot>> GetTodayKotsAsync(int restaurantId);
    }
}
