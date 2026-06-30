using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;
using TourGuideApp.Services;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/audios")]
    public class AudioController : ControllerBase
    {
        private readonly TourGuideContext _context;
        private readonly AudioService _audioService;

        public AudioController(TourGuideContext context, AudioService audioService)
        {
            _context = context;
            _audioService = audioService;
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
        // Tự động generate audio từ transcript nếu không cung cấp audio URL
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

            // Tự động generate audio từ transcript nếu không cung cấp audio URL
            var (finalAudioUrl, finalTranscript) = await _audioService.AutoGenerateAudioAsync(
                audio.POIId,
                audio.LanguageCode,
                audio.TranscriptText,
                audio.AudioUrl
            );

            if (existingAudio != null)
            {
                // Xóa file audio cũ nếu là file local
                await _audioService.DeleteAudioFileAsync(existingAudio.AudioUrl);

                // Update existing audio
                existingAudio.AudioUrl = finalAudioUrl ?? existingAudio.AudioUrl;
                existingAudio.TranscriptText = finalTranscript;
                await _context.SaveChangesAsync();
                return Ok(existingAudio);
            }

            // Tạo bản audio mới
            var newAudio = new Audio
            {
                POIId = audio.POIId,
                LanguageCode = audio.LanguageCode,
                AudioUrl = finalAudioUrl ?? string.Empty,
                TranscriptText = finalTranscript
            };

            _context.Audios.Add(newAudio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByPoi), new { poiId = newAudio.POIId }, newAudio);
        }

        // PUT /api/audios/{id} -> sửa link audio / transcript - ADMIN hoặc VENDOR
        // Tự động generate audio từ transcript nếu không cung cấp audio URL
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

            // Tự động generate audio từ transcript nếu không cung cấp audio URL
            var (finalAudioUrl, finalTranscript) = await _audioService.AutoGenerateAudioAsync(
                audio.POIId,
                audio.LanguageCode,
                updated.TranscriptText,
                updated.AudioUrl
            );

            // Xóa file audio cũ nếu là file local và sẽ được thay thế
            if (!string.IsNullOrEmpty(finalAudioUrl) && finalAudioUrl != audio.AudioUrl)
            {
                await _audioService.DeleteAudioFileAsync(audio.AudioUrl);
            }

            audio.AudioUrl = finalAudioUrl ?? audio.AudioUrl;
            audio.TranscriptText = finalTranscript;
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
