using BillByte.Interface;
using BillByte.Repositories.Interface;
using Billbyte_BE.Helpers;
using Billbyte_BE.Models;
using Billbyte_BE.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Billbyte_BE.Controllers
{
    [ApiController]
    [Route("api/table-preferences")]
    public class TablePreferencesController : ControllerBase
    {
        private readonly ITablePreferenceRepository _repository;
        private readonly IUserTableAssignmentRepository _assignmentRepo;

        public TablePreferencesController(
            ITablePreferenceRepository repository,
            IUserTableAssignmentRepository assignmentRepo)
        {
            _repository = repository;
            _assignmentRepo = assignmentRepo;
        }

        // 🔐 ROLE-AWARE LOAD
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var restaurantId = User.RestaurantId();
            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            // ✅ Owner / Admin → ALL sections
            if (role == "Owner" || role == "Admin")
            {
                return Ok(await _repository.GetAllAsync(restaurantId));
            }

            // ✅ Other roles → ONLY assigned sections
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var sections = await _assignmentRepo
                .GetSectionsForUserAsync(restaurantId, userId);

            return Ok(sections);
        }

        // ❌ NO CHANGE BELOW THIS

        [HttpPost]
        public async Task<IActionResult> Post(List<TablePreference> request)
        {
            var restaurantId = User.RestaurantId();
            request.ForEach(x => x.RestaurantId = restaurantId);
            await _repository.AddRangeAsync(request);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, TablePreference request)
        {
            var restaurantId = User.RestaurantId();

            var existing = await _repository.GetByIdAsync(id, restaurantId);
            if (existing == null)
                return NotFound();

            existing.Name = request.Name;
            existing.TableCount = request.TableCount;

            await _repository.UpdateAsync(existing);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var restaurantId = User.RestaurantId();
            return Ok(await _repository.DeleteAsync(id, restaurantId));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            var restaurantId = User.RestaurantId();
            return Ok(await _repository.DeleteAllAsync(restaurantId));
        }
    }
}
