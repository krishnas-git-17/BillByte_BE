using BillByte.DTO;
using BillByte.Helpers;
using BillByte.Models;
using BillByte.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BillByte.Controllers
{
    [ApiController]
    [Authorize(Roles = "Owner,Admin")]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;

        public UsersController(IUserRepository repo)
        {
            _repo = repo;
        }

        // ✅ List users (restaurant-scoped)
        [HttpGet]
        public IActionResult GetUsers()
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            return Ok(_repo.GetByRestaurant(restaurantId));
        }

        // ✅ Create user (auto EmployeeId + password)
        [HttpPost]
        public IActionResult Create(CreateUserRequestDto dto)
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            // ✅ FIXED
            var creatorId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            );

            var role = (UserRole)dto.Role;

            var sequence = _repo.GetNextEmployeeSequence(restaurantId, role);

            var employeeId = EmployeeIdGenerator.Generate(
                restaurantId,
                role,
                sequence
            );

            var rawPassword = PasswordGenerator.Generate();

            var user = new User
            {
                RestaurantId = restaurantId,
                EmployeeId = employeeId,
                Name = dto.Name,
                Email = string.IsNullOrWhiteSpace(dto.Email)
    ? null
    : dto.Email.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
                Role = role,
                CreatedByUserId = creatorId,
                ForcePasswordChange = true,
                IsActive = true
            };

            _repo.Add(user);
            _repo.Save();

            return Ok(new
            {
                employeeId,
                password = rawPassword
            });
        }

        // ✅ Enable / Disable user (restaurant safe)
        [HttpPatch("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] bool isActive)
        {
            var restaurantId = int.Parse(
                User.FindFirst("restaurantId")!.Value
            );

            var user = _repo.GetById(id);
            if (user == null || user.RestaurantId != restaurantId)
                return NotFound();

            user.IsActive = isActive;
            _repo.Save();

            return Ok();
        }
    }
}
