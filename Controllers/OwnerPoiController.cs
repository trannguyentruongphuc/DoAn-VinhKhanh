using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/owner/pois")]
    [ApiController]
    [Authorize(Roles = "Vendor,Admin")]
    public class OwnerPoiController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public OwnerPoiController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetMyPois()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            return await _context.POIs.Where(p => p.VendorId == userId).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<POI>> CreatePoi(POI poi)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            poi.VendorId = userId;
            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyPois), new { id = poi.Id }, poi);
        }
    }
}
