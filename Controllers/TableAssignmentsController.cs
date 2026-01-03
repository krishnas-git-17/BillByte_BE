using BillByte.DTO;
using BillByte.Repositories.Interface;
using Billbyte_BE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BillByte.Controllers
{
    [ApiController]
    [Route("api/table-assignments")]
    [Authorize]
    public class TableAssignmentsController : ControllerBase
    {
        private readonly IUserTableAssignmentRepository _repo;
        private readonly IUserRepository _userRepo;


        public TableAssignmentsController(IUserTableAssignmentRepository repo, IUserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpPost]
        public async Task<IActionResult> Assign(AssignTablesRequestDto dto)
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            var user = _userRepo.GetByEmployeeId(dto.EmployeeId);

            if (user == null || user.RestaurantId != restaurantId)
                return BadRequest("Invalid EmployeeId");

            foreach (var sectionId in dto.TablePreferenceIds)
            {
                await _repo.AssignAsync(new UserTableAssignment
                {
                    RestaurantId = restaurantId,
                    UserId = user.Id,
                    TablePreferenceId = sectionId
                });
            }

            return Ok();
        }

        [Authorize(Roles = "Owner,Admin")]
        [HttpGet("by-employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            var user = _userRepo.GetByEmployeeId(employeeId);

            if (user == null || user.RestaurantId != restaurantId)
                return BadRequest("Invalid EmployeeId");

            var ids = await _repo.GetAssignedSectionIdsAsync(
                restaurantId,
                user.Id
            );

            return Ok(ids);
        }

        // Logged-in user loads assigned sections
        [HttpGet("my")]
        public async Task<IActionResult> MySections()
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            // Owner/Admin → load all sections
            if (role == "Owner" || role == "Admin")
                return Ok(); // frontend already uses /table-preferences

            return Ok(
                await _repo.GetSectionsForUserAsync(restaurantId, userId)
            );
        }
    }
}
