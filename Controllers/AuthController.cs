using BillByte.DTO;
using BillByte.Models;
using BillByte.Repositories.Interface;
using BillByte.Services;
using Billbyte_BE.Data;
using Billbyte_BE.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly EmailService _emailService;

        public AuthController(
      IConfiguration config,
      IUserRepository userRepo,
      AppDbContext context,
      EmailService emailService)
        {
            _config = config;
            _userRepo = userRepo;
            _context = context;
            _emailService = emailService;
        }

        // ======================
        // SIGNUP (OWNER)
        // ======================
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupRequestDto request)
        {
            using var tx = await _context.Database.BeginTransactionAsync();

            // 1️⃣ Validate plan
            var plan = await _context.Plans.FindAsync(request.PlanId);
            if (plan == null || !plan.IsActive)
                return BadRequest("Invalid plan selected");

            // 2️⃣ CHECK EMAIL FIRST (THIS WAS MISSING)
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            // ❌ Case: email already verified → block
            if (user != null && user.IsEmailVerified)
            {
                return BadRequest("Email already registered. Please login.");
            }

            // 🔁 Case: email exists but NOT verified → resend OTP
            if (user != null && !user.IsEmailVerified)
            {
                user.EmailOtp = new Random().Next(100000, 999999).ToString();
                user.EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5);

                await _context.SaveChangesAsync();
                await _emailService.SendOtpAsync(user.Email!, user.EmailOtp);

                await tx.CommitAsync();

                return Ok(new
                {
                    message = "OTP resent to your email"
                });
            }

            // ✅ Case: NEW EMAIL → create everything
            var restaurant = new Restaurant
            {
                Name = request.RestaurantName
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            var otp = new Random().Next(100000, 999999).ToString();

            var owner = new User
            {
                RestaurantId = restaurant.Id,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = UserRole.Owner,
                EmployeeId = $"BB-OWN-{restaurant.Id}",
                Name = "Owner",
                IsActive = true,

                IsEmailVerified = false,
                EmailOtp = otp,
                EmailOtpExpiry = DateTime.UtcNow.AddMinutes(5),

                PlanId = plan.Id,
                IsPlanActive = true,
                PlanExpiryDate = DateTime.UtcNow.AddDays(plan.DurationInDays)
            };

            _context.Users.Add(owner);
            await _context.SaveChangesAsync();

            await _emailService.SendOtpAsync(owner.Email!, otp);

            await tx.CommitAsync();

            return Ok(new
            {
                message = "Signup successful. OTP sent to email."
            });
        }



        // ======================
        // VERIFY EMAIL OTP
        // ======================
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailDto request)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
                return BadRequest("User not found");

            if (user.EmailOtp != request.Otp)
                return BadRequest("Invalid OTP");

            if (user.EmailOtpExpiry < DateTime.UtcNow)
                return BadRequest("OTP expired");

            user.IsEmailVerified = true;
            user.EmailOtp = null;
            user.EmailOtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Email verified successfully");
        }

        // ======================
        // LOGIN
        // ======================
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto request)
        {
            User? user = null;

            if (!string.IsNullOrEmpty(request.Email))
                user = _userRepo.GetByEmail(request.Email);
            else if (!string.IsNullOrEmpty(request.EmployeeId))
                user = _userRepo.GetByEmployeeId(request.EmployeeId);

            if (!user.IsEmailVerified)
                return Unauthorized("Email not verified");

            if (!user.IsPlanActive || user.PlanExpiryDate < DateTime.UtcNow)
                return Unauthorized("Plan inactive or expired");


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
