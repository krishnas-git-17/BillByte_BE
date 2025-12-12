using BillByte.Model;
using Billbyte_BE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class MenuItemImageController : ControllerBase
{
    private readonly AppDbContext _context;

    public MenuItemImageController(AppDbContext context)
    {
        _context = context;
    }

    // -------------------- POST: Add Image --------------------
    [HttpPost]
    public async Task<IActionResult> AddImage([FromBody] MenuItemImgs img)
    {
        if (img == null || string.IsNullOrEmpty(img.ItemImage))
            return BadRequest("Invalid image object.");

        _context.menuItemImgs.Add(img);
        await _context.SaveChangesAsync();

        return Ok(img);
    }

    // -------------------- GET ALL --------------------
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _context.menuItemImgs.ToListAsync();
        return Ok(data);
    }

    // -------------------- GET BY ID --------------------
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var img = await _context.menuItemImgs.FindAsync(id);

        if (img == null)
            return NotFound("Image not found.");

        return Ok(img);
    }

    // -------------------- DELETE --------------------
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var img = await _context.menuItemImgs.FindAsync(id);

        if (img == null)
            return NotFound("Image not found.");

        _context.menuItemImgs.Remove(img);
        await _context.SaveChangesAsync();

        return Ok("Deleted successfully.");
    }
}
