using Billbyte_BE.Helpers;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/table-state")]
    public class TableStateController : ControllerBase
    {
        private readonly ITableStateRepository _repo;

        public TableStateController(ITableStateRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var restaurantId = User.RestaurantId();
            return Ok(await _repo.GetAllAsync(restaurantId));
        }

        [HttpPost("occupied/{tableId}")]
        public async Task<IActionResult> SetOccupied(string tableId)
        {
            var restaurantId = User.RestaurantId();
            await _repo.SetOccupiedAsync(tableId, restaurantId);
            return Ok();
        }

        [HttpPost("ordered/{tableId}")]
        public async Task<IActionResult> MoveToOrdered(string tableId)
        {
            var restaurantId = User.RestaurantId();
            var success = await _repo.MoveToOrderedAsync(tableId, restaurantId);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("billing/{tableId}")]
        public async Task<IActionResult> MoveToBilling(string tableId)
        {
            var restaurantId = User.RestaurantId();
            var success = await _repo.MoveToBillingAsync(tableId, restaurantId);
            return success ? Ok() : BadRequest();
        }

        [HttpPost("reset/{tableId}")]
        public async Task<IActionResult> Reset(string tableId)
        {
            var restaurantId = User.RestaurantId();
            await _repo.ResetAsync(tableId, restaurantId);
            return Ok();
        }
    }
}
