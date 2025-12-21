using BillByte.Interface;
using BillByte.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/menu-item-images")]
public class MenuItemImageController : ControllerBase
{
    private readonly IMenuItemImagesRepository _repo;

    public MenuItemImageController(IMenuItemImagesRepository repo)
    {
        _repo = repo;
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> Add(MenuItemImgs img)
    {
        if (string.IsNullOrWhiteSpace(img.ItemImage))
            return BadRequest("Image is required");

        var result = await _repo.AddAsync(img);
        return Ok(result);
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _repo.GetAllAsync());
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var data = await _repo.GetByIdAsync(id);
        return data == null ? NotFound() : Ok(data);
    }

    // PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MenuItemImgs img)
    {
        if (id != img.Id)
            return BadRequest("ID mismatch");

        var updated = await _repo.UpdateAsync(img);
        return Ok(updated);
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repo.DeleteAsync(id);
        return success ? Ok("Deleted") : NotFound();
    }
}
