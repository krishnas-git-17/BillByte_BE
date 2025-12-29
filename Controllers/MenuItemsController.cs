using Billbyte_BE.Helpers;
using BillByte.Interface;
using BillByte.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillByte.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/menu-items")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemRepository _repo;

        public MenuItemsController(IMenuItemRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var restaurantId = User.RestaurantId();
            return Ok(await _repo.GetAllAsync(restaurantId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var restaurantId = User.RestaurantId();
            var item = await _repo.GetByIdAsync(id, restaurantId);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MenuItem item)
        {
            item.RestaurantId = User.RestaurantId();
            item.CreatedBy = User.UserId();

            return Ok(await _repo.AddAsync(item));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MenuItem item)
        {
            item.MenuId = id;
            item.RestaurantId = User.RestaurantId();

            return Ok(await _repo.UpdateAsync(item));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var restaurantId = User.RestaurantId();
            return Ok(await _repo.DeleteAsync(id, restaurantId));
        }
    }
}
