using BillByte.DTO;
using BillByte.Models;
using BillByte.Repositories.Interface;
using Billbyte_BE.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BillByte.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _userRepo;
        private readonly AppDbContext _context;

        public AuthController(
            IConfiguration config,
            IUserRepository userRepo,
            AppDbContext context)
        {
            _config = config;
            _userRepo = userRepo;
            _context = context;
        }

        // ======================
        // SIGNUP (OWNER ONLY)
        // ======================
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequestDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            // 1️⃣ Create restaurant
            var restaurant = new Restaurant
            {
                Name = request.RestaurantName
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // 2️⃣ Create owner user
            var owner = new User
            {
                RestaurantId = restaurant.Id,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Owner,
                EmployeeId = $"BB-OWN-{restaurant.Id}",
                Name = "Owner",
                IsActive = true,
                ForcePasswordChange = false
            };

            _context.Users.Add(owner);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            return Ok(new
            {
                restaurantId = restaurant.Id,
                ownerEmployeeId = owner.EmployeeId
            });
        }

        // ======================
        // LOGIN
        // ======================
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            User? user = null;

            // EMAIL LOGIN
            if (!string.IsNullOrEmpty(request.Email))
            {
                user = _userRepo.GetByEmail(request.Email);
            }
            // EMPLOYEE ID LOGIN
            else if (!string.IsNullOrEmpty(request.EmployeeId))
            {
                user = _userRepo.GetByEmployeeId(request.EmployeeId);
            }

            if (user == null || !user.IsActive)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            user.LastLoginAt = DateTime.UtcNow;
            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("restaurantId", user.RestaurantId.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("name", user.Name)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_config["Jwt:ExpiryMinutes"])
                ),
                signingCredentials: new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                )
            );

            return Ok(new LoginResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresIn = Convert.ToInt32(_config["Jwt:ExpiryMinutes"]) * 60,
                UserId = user.Id,
                RestaurantId = user.RestaurantId,
                Name = user.Name,
                Role = user.Role.ToString(),
                ForcePasswordChange = user.ForcePasswordChange
            });
        }
    }
}
