using BillByte.Interface;
using BillByte.Model;
using Microsoft.AspNetCore.Mvc;

namespace BillByte.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemRepository _repo;

        public MenuItemsController(IMenuItemRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> Get() => Ok(await _repo.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) => Ok(await _repo.GetByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create(MenuItem item)
        {
            var created = await _repo.AddAsync(item);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MenuItem item)
        {
            item.ItemId = id;
            return Ok(await _repo.UpdateAsync(item));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            return Ok(await _repo.DeleteAsync(id));
        }

        [HttpGet("foodtype/{typeId}")]
        public async Task<IActionResult> GetByFoodType(int typeId)
        {
            return Ok(await _repo.GetByFoodTypeAsync(typeId));
        }
    }
}
