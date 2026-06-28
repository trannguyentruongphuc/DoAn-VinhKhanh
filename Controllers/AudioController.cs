using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/audios")]
    public class AudioController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public AudioController(TourGuideContext context)
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

        // GET /api/audios -> toàn bộ bản audio (mọi người đều xem được)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Audio>>> GetAll()
        {
            var audios = await _context.Audios.OrderBy(a => a.POIId).ToListAsync();
            return Ok(audios);
        }

        // GET /api/audios/poi/{poiId} -> tất cả bản audio (mọi người đều xem được)
        [HttpGet("poi/{poiId}")]
        public async Task<ActionResult<IEnumerable<Audio>>> GetByPoi(int poiId)
        {
            var audios = await _context.Audios
                .Where(a => a.POIId == poiId)
                .ToListAsync();
            return Ok(audios);
        }

        // POST /api/audios -> gán audio - ADMIN hoặc VENDOR
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPost]
        public async Task<ActionResult<Audio>> Create([FromBody] Audio audio)
        {
            var poi = await _context.POIs.FindAsync(audio.POIId);
            if (poi == null)
            {
                return BadRequest(new { message = $"POIId {audio.POIId} không tồn tại." });
            }

            // Vendor chỉ được thêm audio cho POI của mình
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi.VendorId != userId)
            {
                return Forbid();
            }

            // Tránh trùng: 1 POI chỉ nên có 1 bản audio cho mỗi ngôn ngữ
            var existingAudio = await _context.Audios
                .FirstOrDefaultAsync(a => a.POIId == audio.POIId && a.LanguageCode == audio.LanguageCode);

            if (existingAudio != null)
            {
                // Update existing audio
                existingAudio.AudioUrl = audio.AudioUrl;
                existingAudio.TranscriptText = audio.TranscriptText;
                await _context.SaveChangesAsync();
                return Ok(existingAudio);
            }

            _context.Audios.Add(audio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByPoi), new { poiId = audio.POIId }, audio);
        }

        // PUT /api/audios/{id} -> sửa link audio / transcript - ADMIN hoặc VENDOR
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Audio updated)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            var poi = await _context.POIs.FindAsync(audio.POIId);
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi?.VendorId != userId)
            {
                return Forbid();
            }

            audio.AudioUrl = updated.AudioUrl;
            audio.TranscriptText = updated.TranscriptText;
            audio.LanguageCode = updated.LanguageCode;

            await _context.SaveChangesAsync();
            return Ok(audio);
        }

        // DELETE /api/audios/{id} - ADMIN hoặc VENDOR
        [Authorize(Roles = "Admin,Vendor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            var poi = await _context.POIs.FindAsync(audio.POIId);
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi?.VendorId != userId)
            {
                return Forbid();
            }

            _context.Audios.Remove(audio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
