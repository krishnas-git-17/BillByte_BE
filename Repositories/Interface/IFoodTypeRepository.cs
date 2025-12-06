using Billbyte_BE.Models;

namespace Billbyte_BE.Repositories.Interface
{
    public interface IFoodTypeRepository
    {
        Task<List<FoodType>> GetAllAsync();
        Task<FoodType?> GetByIdAsync(int id);
        Task<FoodType> CreateAsync(FoodType type);
        Task<FoodType> UpdateAsync(FoodType type);
        Task<bool> DeleteAsync(int id);
    }
}
