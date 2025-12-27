using Billbyte_BE.Helpers;
using BillByte.Interface;
using BillByte.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/menu-item-images")]
public class MenuItemImageController : ControllerBase
{
    private readonly IMenuItemImagesRepository _repo;

    public MenuItemImageController(IMenuItemImagesRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurantId = User.RestaurantId();
        var images = await _repo.GetAllAsync(restaurantId);
        return Ok(images);
    }


    [HttpPost]
    public async Task<IActionResult> Add(MenuItemImgs img)
    {
        img.RestaurantId = User.RestaurantId();
        img.CreatedBy = User.UserId();

        return Ok(await _repo.AddAsync(img));
    }

    [HttpGet("{menuId}")]
    public async Task<IActionResult> GetByMenu(string menuId)
    {
        var restaurantId = User.RestaurantId();
        return Ok(await _repo.GetByMenuIdAsync(menuId, restaurantId));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var restaurantId = User.RestaurantId();
        return Ok(await _repo.DeleteAsync(id, restaurantId));
    }
}
