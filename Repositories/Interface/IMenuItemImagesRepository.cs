using BillByte.Model;

namespace BillByte.Interface
{
    public interface IMenuItemImagesRepository
    {
        Task<List<MenuItemImgs>> GetAllAsync(int restaurantId);
        Task<List<MenuItemImgs>> GetByMenuIdAsync(string menuId, int restaurantId);
        Task<MenuItemImgs> AddAsync(MenuItemImgs item);
        Task<bool> DeleteAsync(int id, int restaurantId);
    }
}
