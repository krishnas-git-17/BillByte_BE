using BillByte.Model;
using Billbyte_BE.Helpers;
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
        order.RestaurantId = User.RestaurantId();
        await _repo.AddOrderAsync(order);

        return Ok(new
        {
            order.Id,
            order.InvoiceNo
        });
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurantId = User.RestaurantId();
        return Ok(await _repo.GetAllAsync(restaurantId));
    }

    [HttpGet("by-invoice/{invoiceNo}")]
    public async Task<IActionResult> GetByInvoice(string invoiceNo)
    {
        var restaurantId = User.RestaurantId();
        var order = await _repo.GetByInvoiceAsync(restaurantId, invoiceNo);
        return order == null ? NotFound() : Ok(order);
    }

}
