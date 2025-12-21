using BillByte.Model;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
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
        await _repo.AddOrderAsync(order);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _repo.GetAllAsync());
    }
}
