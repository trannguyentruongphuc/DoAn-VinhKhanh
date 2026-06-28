using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public POIController(TourGuideContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        private string? GetCurrentRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value;
        }

        // GET /api/pois/my -> POIs của Vendor đang đăng nhập
        [Authorize(Roles = "Vendor")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<POI>>> GetMyPOIs()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var pois = await _context.POIs
                .Include(p => p.Audios)
                .Where(p => p.VendorId == userId)
                .OrderBy(p => p.Id)
                .ToListAsync();
            return Ok(pois);
        }

        // GET /api/pois -> danh sách toàn bộ POI (ai cũng xem được)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POI>>> GetAll()
        {
            var pois = await _context.POIs
                .Include(p => p.Audios)
                .OrderBy(p => p.Id)
                .ToListAsync();
            return Ok(pois);
        }

        // GET /api/pois/{id} -> 1 điểm cụ thể (ai cũng xem được)
        [HttpGet("{id}")]
        public async Task<ActionResult<POI>> GetById(int id)
        {
            var poi = await _context.POIs
                .Include(p => p.Audios)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poi == null) return NotFound();
            return Ok(poi);
        }

        // POST /api/pois -> thêm điểm mới - ADMIN hoặc VENDOR
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPost]
        public async Task<ActionResult<POI>> Create([FromBody] POI poi)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = GetCurrentRole();
            if (role == "Vendor")
            {
                var userId = GetCurrentUserId();
                poi.VendorId = userId;
            }

            _context.POIs.Add(poi);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
        }

        // PUT /api/pois/{id} -> sửa thông tin điểm - ADMIN hoặc VENDOR (chỉ POI của mình)
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] POI updated)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            var role = GetCurrentRole();
            var userId = GetCurrentUserId();

            if (role == "Vendor" && poi.VendorId != userId)
            {
                return Forbid();
            }

            poi.Name = updated.Name;
            poi.Description = updated.Description;
            poi.Lat = updated.Lat;
            poi.Lng = updated.Lng;
            poi.Radius = updated.Radius;
            poi.Priority = updated.Priority;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/pois/{id} -> xóa điểm - ADMIN hoặc VENDOR (chỉ POI của mình)
        [Authorize(Roles = "Admin,Vendor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            var role = GetCurrentRole();
            var userId = GetCurrentUserId();

            if (role == "Vendor" && poi.VendorId != userId)
            {
                return Forbid();
            }

            _context.POIs.Remove(poi);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
