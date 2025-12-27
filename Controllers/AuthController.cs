using BillByte.DTO;
using BillByte.Models;
using BillByte.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Billbyte_BE.Data;

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

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var restaurant = new Restaurant
            {
                Name = request.RestaurantName
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var user = new User
            {
                RestaurantId = restaurant.Id,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = "Owner",
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok();
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            var user = _userRepo.GetByEmail(request.Email);

            if (user == null || !user.IsActive)
                return Unauthorized();

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("restaurantId", user.RestaurantId.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
                Email = user.Email,
                Role = user.Role
            });
        }
    }
}
