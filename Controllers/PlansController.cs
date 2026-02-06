using Billbyte_BE.Data;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/plans")]
    public class PlansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlansController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetPlans()
        {
            var plans = _context.Plans
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.MaxUsers,
                    p.DurationInDays
                });

            return Ok(plans);
        }
    }

}
