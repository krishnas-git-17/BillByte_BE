using Billbyte_BE.Data;
using Billbyte_BE.DTO;
using Billbyte_BE.Helpers;
using Billbyte_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/kot")]
    public class KotController : ControllerBase
    {
        private readonly AppDbContext _context;

        public KotController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("{tableId}")]
        public async Task<IActionResult> SendKot(string tableId)
        {
            var restaurantId = User.RestaurantId();

            // 1️⃣ Current active items
            var activeItems = await _context.ActiveTableItems
                .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
                .ToListAsync();

            if (!activeItems.Any())
                return BadRequest("No items to send to kitchen");

            // 2️⃣ Previous KOT snapshot
            var snapshots = await _context.KotSnapshots
                .Where(x => x.TableId == tableId && x.RestaurantId == restaurantId)
                .ToListAsync();

            var kotOutput = new List<KotItemDto>();

            foreach (var item in activeItems)
            {
                var snap = snapshots.FirstOrDefault(x => x.ItemId == item.ItemId);
                var previousQty = snap?.Qty ?? 0;
                var diff = item.Qty - previousQty;

                if (diff != 0)
                {
                    kotOutput.Add(new KotItemDto
                    {
                        ItemId = item.ItemId,
                        ItemName = item.ItemName,
                        QtyChange = diff
                    });
                }
            }

            // 3️⃣ Handle removed items
            foreach (var snap in snapshots)
            {
                if (!activeItems.Any(x => x.ItemId == snap.ItemId))
                {
                    kotOutput.Add(new KotItemDto
                    {
                        ItemId = snap.ItemId,
                        ItemName = "REMOVED",
                        QtyChange = -snap.Qty
                    });
                }
            }

            if (!kotOutput.Any())
                return Ok(new { message = "No KOT changes" });

            // 4️⃣ Update snapshots
            _context.KotSnapshots.RemoveRange(snapshots);

            foreach (var item in activeItems)
            {
                _context.KotSnapshots.Add(new KotSnapshot
                {
                    RestaurantId = restaurantId,
                    TableId = tableId,
                    ItemId = item.ItemId,
                    Qty = item.Qty
                });
            }

            await _context.SaveChangesAsync();

            // 5️⃣ Return KOT payload (Kitchen will print this)
            return Ok(kotOutput);
        }
    }
}
