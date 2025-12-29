using BillByte.Models;
using Billbyte_BE.DTO;
using Billbyte_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/active-table-items")]
public class ActiveTableItemsController : ControllerBase
{
    private readonly IActiveTableItemRepository _repo;

    public ActiveTableItemsController(IActiveTableItemRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{tableId}")]
    public async Task<IActionResult> GetByTable(string tableId)
    {
        var restaurantId = User.RestaurantId();
        return Ok(await _repo.GetByTableAsync(tableId, restaurantId));
    }

    [HttpPost("{tableId}")]
    public async Task<IActionResult> AddItem(
     string tableId,
     [FromBody] ActiveTableItemCreateDto dto)
    {
        var item = new ActiveTableItem
        {
            TableId = tableId,
            RestaurantId = User.RestaurantId(),
            ItemId = dto.ItemId,
            ItemName = dto.ItemName,
            Price = dto.Price,
            Qty = dto.Qty
        };

        await _repo.AddOrUpdateAsync(item);
        return Ok();
    }


    [HttpPut("{tableId}/{itemId}")]
    public async Task<IActionResult> UpdateQty(
       string tableId,
       int itemId,
       [FromBody] UpdateQtyDto dto
   )
    {
        await _repo.UpdateQtyAsync(
            tableId,
            itemId,
            dto.Qty,
            User.RestaurantId()
        );

        return Ok();
    }


    [HttpDelete("{tableId}/{itemId}")]
    public async Task<IActionResult> DeleteItem(string tableId, int itemId)
    {
        await _repo.DeleteItemAsync(tableId, itemId, User.RestaurantId());
        return Ok();
    }

    [HttpDelete("clear/{tableId}")]
    public async Task<IActionResult> ClearTable(string tableId)
    {
        await _repo.ClearTableAsync(tableId, User.RestaurantId());
        return Ok();
    }
}
