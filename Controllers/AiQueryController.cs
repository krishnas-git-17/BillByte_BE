using Billbyte_BE.Data;
using Billbyte_BE.Helpers;
using Billbyte_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AiQueryController : ControllerBase
    {
        private readonly GeminiAiService _ai;
        private readonly AppDbContext _db;

        public AiQueryController(GeminiAiService ai, AppDbContext db)
        {
            _ai = ai;
            _db = db;
        }

        [HttpPost("query-db")]
        public async Task<IActionResult> QueryDatabase([FromBody] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return BadRequest("Query text required");

            var restaurantId = User.RestaurantId();
            // 1️⃣ Generate SQL (already correct & quoted)
            var sql = await _ai.GenerateSqlAsync(text);

            // 2️⃣ Normalize whitespace only
            sql = sql
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();

            Console.WriteLine("AI SQL => " + sql);

            // 3️⃣ Validate (no lowercase, no replace)
            if (!SqlValidator.IsSafe(sql))
                return BadRequest("Unsafe SQL generated");

            sql = InjectRestaurantFilter(sql, restaurantId);

            // 4️⃣ Execute (works with EF tables)
            var data = await _db.ExecuteSqlAsync(sql);

            return Ok(new
            {
                generatedSql = sql,
                result = data
            });
        }

        private static string InjectRestaurantFilter(string sql, int restaurantId)
        {
             var lower = sql.ToLower();

            if (lower.Contains(" where "))
            {
                // Insert AND before LIMIT
                return sql.Replace(
                    " limit",
                    $" AND \"RestaurantId\" = {restaurantId} limit",
                    StringComparison.OrdinalIgnoreCase
                );
            }
            else
            {
                // Insert WHERE before LIMIT
                return sql.Replace(
                    " limit",
                    $" WHERE \"RestaurantId\" = {restaurantId} limit",
                    StringComparison.OrdinalIgnoreCase
                );
            }
        }

    }
}
