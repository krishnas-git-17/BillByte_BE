using Billbyte_BE.Helpers;
using BillByte.Model;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/completed-orders")]
public class CompletedOrdersController : ControllerBase
{
    private readonly ICompletedOrderRepository _repo;

    public CompletedOrdersController(ICompletedOrderRepository repo)
    {
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> SaveOrder([FromBody] CompletedOrder order)
    {
        order.RestaurantId = User.RestaurantId();
        await _repo.AddOrderAsync(order);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurantId = User.RestaurantId();
        return Ok(await _repo.GetAllAsync(restaurantId));
    }
}
