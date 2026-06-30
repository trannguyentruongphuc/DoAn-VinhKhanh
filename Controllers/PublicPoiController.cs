using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/public/pois")]
    [ApiController]
    public class PublicPoiController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public PublicPoiController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetPublicPois()
        {
            return await _context.POIs
                .Include(p => p.Audios)
                .Include(p => p.Localizations)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetPoi(int id)
        {
            var poi = await _context.POIs
                .Include(p => p.Audios)
                .Include(p => p.Localizations)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poi == null) return NotFound();
            return poi;
        }
    }
}
