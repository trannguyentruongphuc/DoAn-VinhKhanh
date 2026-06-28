using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TourGuideContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(TourGuideContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username và password không được để trống" });
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Username đã tồn tại" });
            }

            var user = new User
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Email = request.Email ?? "",
                Role = request.Role == "Vendor" ? "Vendor" : "User",  // Hỗ trợ Vendor
                StoreName = request.Role == "Vendor" ? request.StoreName : null,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username và password không được để trống" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Username hoặc password không đúng" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "Tài khoản đã bị khóa. Vui lòng liên hệ Admin." });
            }

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Đăng nhập thành công",
                token = token,
                user = new
                {
                    id = user.Id,
                    username = user.Username,
                    email = user.Email,
                    role = user.Role,
                    storeName = user.StoreName
                }
            });
        }

        [HttpGet("me")]
        public async Task<ActionResult> GetCurrentUser()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy user" });
            }

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = user.Role,
                storeName = user.StoreName,
                createdAt = user.CreatedAt
            });
        }

        // PUT /api/auth/profile - Cập nhật profile (Vendor)
        [Authorize(Roles = "Vendor")]
        [HttpPut("profile")]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Không xác định được user" });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy user" });
            }

            if (!string.IsNullOrEmpty(request.StoreName))
                user.StoreName = request.StoreName;
            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                storeName = user.StoreName,
                email = user.Email
            });
        }

        // GET /api/auth/users - Danh sách toàn bộ người dùng (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    id = u.Id,
                    username = u.Username,
                    email = u.Email,
                    role = u.Role,
                    storeName = u.StoreName,
                    createdAt = u.CreatedAt,
                    isActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        // PUT /api/auth/user/{id}/status - Khóa/mở khóa tài khoản (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPut("user/{id}/status")]
        public async Task<ActionResult> UpdateUserStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy user" });
            }

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = user.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản" });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = "ThisIsAVeryLongSecretKeyForTourGuideApp2024!@#$%^&*()";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "TourGuideApp",
                audience: "TourGuideApp",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? StoreName { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? StoreName { get; set; }
        public string? Email { get; set; }
    }

    public class UpdateStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
