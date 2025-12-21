using BillByte.Interface;
using BillByte.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillByte.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly IMenuItemRepository _repo;

        public MenuItemsController(IMenuItemRepository repo)
        {
            _repo = repo;
        }

        // -------------------------------- GET ALL --------------------------------
        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _repo.GetAllAsync());

        // -------------------------------- GET BY ID --------------------------------
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return NotFound("Menu item not found");

            return Ok(item);
        }

        // -------------------------------- CREATE --------------------------------
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MenuItem item)
        {
            var created = await _repo.AddAsync(item);
            return Ok(created);
        }

        // -------------------------------- UPDATE --------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MenuItem item)
        {
            item.MenuId = id;
            var updated = await _repo.UpdateAsync(item);
            return Ok(updated);
        }

        // -------------------------------- DELETE --------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            bool deleted = await _repo.DeleteAsync(id);
            return Ok(new { deleted });
        }
    }
}
