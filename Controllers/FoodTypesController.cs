using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoodTypesController : ControllerBase
    {
        private readonly IFoodTypeRepository _repo;

        public FoodTypesController(IFoodTypeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _repo.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var ft = await _repo.GetByIdAsync(id);
            return ft == null ? NotFound() : Ok(ft);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FoodType type)
        {
            var created = await _repo.CreateAsync(type);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, FoodType type)
        {
            type.FoodTypeId = id;
            var updated = await _repo.UpdateAsync(type);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repo.DeleteAsync(id);
            return success ? Ok() : NotFound();
        }
    }
}
