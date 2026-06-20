using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/POI")]
    public class POIController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public POIController(TourGuideContext context)
        {
            _context = context;
        }

        // GET /api/POI -> danh sách toàn bộ POI (kèm audios để Admin hiển thị)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll()
        {
            var pois = await _context.POIs
                .Include(p => p.Audios)
                .OrderBy(p => p.Id)
                .ToListAsync();
            return Ok(pois);
        }

        // GET /api/POI/{id} -> 1 điểm cụ thể, kèm audios (TV3/Người-B dùng khi quét QR)
        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetById(int id)
        {
            var poi = await _context.POIs
                .Include(p => p.Audios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poi == null) return NotFound();
            return Ok(poi);
        }

        // POST /api/POI -> thêm điểm mới
        [HttpPost]
        public async Task<ActionResult<POI>> Create([FromBody] POI poi)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
        }

        // PUT /api/POI/{id} -> sửa thông tin điểm
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] POI updated)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            poi.Name = updated.Name;
            poi.Description = updated.Description;
            poi.Lat = updated.Lat;
            poi.Lng = updated.Lng;
            poi.Radius = updated.Radius;
            poi.Priority = updated.Priority;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/POI/{id} -> xóa điểm
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
