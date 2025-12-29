using Billbyte_BE.Helpers;
using BillByte.Model;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/kot")]
public class KotController : ControllerBase
{
    private readonly IKotRepository _repo;

    public KotController(IKotRepository repo)
    {
        _repo = repo;
    }

    [HttpPost]
    public async Task<IActionResult> CreateKOT([FromBody] KotSnapshot kot)
    {
        kot.RestaurantId = User.RestaurantId();

        var saved = await _repo.CreateKotAsync(kot);

        return Ok(new
        {
            saved.Id,
            saved.KotNo,
            saved.TableId
        });
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetTodayKots()
    {
        var rid = User.RestaurantId();
        return Ok(await _repo.GetTodayKotsAsync(rid));
    }
}
