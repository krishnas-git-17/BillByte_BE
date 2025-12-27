using BillByte.Interface;
using BillByte.Model;
using Billbyte_BE.Data;
using Microsoft.EntityFrameworkCore;

namespace BillByte.Repository
{
    public class MenuItemImageRepository : IMenuItemImagesRepository
    {
        private readonly AppDbContext _context;

        // ✅ SINGLE constructor (DI-safe)
        public MenuItemImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<MenuItemImgs>> GetAllAsync(int restaurantId)
        {
            return await _context.MenuItemImgs
                .Where(x => x.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<List<MenuItemImgs>> GetByMenuIdAsync(string menuId, int restaurantId)
        {
            return await _context.MenuItemImgs
                .Where(x => x.MenuId == menuId && x.RestaurantId == restaurantId)
                .ToListAsync();
        }

        public async Task<MenuItemImgs> AddAsync(MenuItemImgs img)
        {
            _context.MenuItemImgs.Add(img);
            await _context.SaveChangesAsync();
            return img;
        }

        public async Task<bool> DeleteAsync(int id, int restaurantId)
        {
            var img = await _context.MenuItemImgs
                .FirstOrDefaultAsync(x => x.Id == id && x.RestaurantId == restaurantId);

            if (img == null) return false;

            _context.MenuItemImgs.Remove(img);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
