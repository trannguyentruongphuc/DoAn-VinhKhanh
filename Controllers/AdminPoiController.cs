using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/admin/pois")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPoiController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public AdminPoiController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAllPois()
        {
            return await _context.POIs.Include(p => p.Vendor).ToListAsync();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePoi(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
