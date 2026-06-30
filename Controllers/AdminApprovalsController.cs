using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/admin/approvals")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminApprovalsController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public AdminApprovalsController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet("registrations")]
        public async Task<ActionResult<IEnumerable<PoiOwnerRegistration>>> GetPendingRegistrations()
        {
            return await _context.PoiOwnerRegistrations
                .Where(r => r.Status == "Pending")
                .Include(r => r.User)
                .ToListAsync();
        }

        [HttpPost("registrations/{id}/approve")]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var reg = await _context.PoiOwnerRegistrations.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (reg == null) return NotFound();

            reg.Status = "Approved";
            if (reg.User != null)
            {
                reg.User.Role = "Vendor";
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Registration approved" });
        }
    }
}
