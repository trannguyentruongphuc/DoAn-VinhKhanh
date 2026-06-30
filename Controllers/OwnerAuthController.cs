using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/owner/auth")]
    [ApiController]
    [Authorize]
    public class OwnerAuthController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public OwnerAuthController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsOwner([FromBody] PoiOwnerRegistration registration)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            registration.UserId = userId;
            registration.CreatedAt = DateTime.UtcNow;
            registration.Status = "Pending";

            _context.PoiOwnerRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration submitted successfully" });
        }
    }
}
