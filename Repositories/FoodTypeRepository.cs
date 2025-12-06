using Billbyte_BE.Data;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Repositories
{
    public class FoodTypeRepository : IFoodTypeRepository
    {
        private readonly AppDbContext _db;

        public FoodTypeRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<FoodType>> GetAllAsync()
        {
            return await _db.FoodTypes.OrderBy(f => f.DisplayOrder).ToListAsync();
        }

        public async Task<FoodType?> GetByIdAsync(int id)
        {
            return await _db.FoodTypes.FindAsync(id);
        }

        public async Task<FoodType> CreateAsync(FoodType type)
        {
            _db.FoodTypes.Add(type);
            await _db.SaveChangesAsync();
            return type;
        }

        public async Task<FoodType> UpdateAsync(FoodType type)
        {
            _db.FoodTypes.Update(type);
            await _db.SaveChangesAsync();
            return type;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ft = await _db.FoodTypes.FindAsync(id);
            if (ft == null) return false;

            _db.FoodTypes.Remove(ft);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
