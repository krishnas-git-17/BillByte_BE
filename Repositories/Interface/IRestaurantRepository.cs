using BillByte.Models;

namespace BillByte.Repositories.Interface
{
    public interface IRestaurantRepository
    {
        Task<Restaurant> CreateAsync(Restaurant restaurant);
    }
}
