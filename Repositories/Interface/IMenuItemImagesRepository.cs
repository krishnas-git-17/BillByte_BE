using BillByte.Model;

namespace BillByte.Interface
{
    public interface IMenuItemImagesRepository
    {
        Task<List<MenuItemImgs>> GetAllAsync();
        Task<MenuItemImgs?> GetByIdAsync(int id);
        Task<MenuItemImgs> AddAsync(MenuItemImgs item);
        Task<MenuItemImgs> UpdateAsync(MenuItemImgs item);
        Task<bool> DeleteAsync(int id);
    }
}
