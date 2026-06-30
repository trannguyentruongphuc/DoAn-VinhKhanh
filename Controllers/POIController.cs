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
    [Route("api/[controller]")]
    public class POIController : ControllerBase
    {
        private readonly TourGuideContext _context;
        private readonly AudioService _audioService;

        public POIController(TourGuideContext context, AudioService audioService)
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
                .Include(p => p.Localizations)
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
                .Include(p => p.Localizations)
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

            // Tự động tạo audio từ description nếu có
            if (!string.IsNullOrWhiteSpace(poi.Description))
            {
                var audioUrl = await _audioService.GenerateAudioFileAsync(
                    poi.Description, "vi", poi.Id, "vi"
                );

                _context.Audios.Add(new Audio
                {
                    POIId = poi.Id,
                    LanguageCode = "vi",
                    TranscriptText = poi.Description,
                    AudioUrl = audioUrl ?? string.Empty
                });
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetById), new { id = poi.Id }, poi);
        }

        // PUT /api/pois/{id} -> sửa thông tin điểm - ADMIN hoặc VENDOR (chỉ POI của mình)
        // Tự động regenerate audio nếu description thay đổi
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

            var descriptionChanged = poi.Description != updated.Description;

            poi.Name = updated.Name;
            poi.Description = updated.Description;
            poi.Lat = updated.Lat;
            poi.Lng = updated.Lng;
            poi.Radius = updated.Radius;
            poi.Priority = updated.Priority;

            // Tự động regenerate audio nếu description thay đổi
            if (descriptionChanged && !string.IsNullOrWhiteSpace(updated.Description))
            {
                // Lấy hoặc tạo audio cho ngôn ngữ mặc định (vi)
                var audio = await _context.Audios
                    .FirstOrDefaultAsync(a => a.POIId == id && a.LanguageCode == "vi");

                var newAudioUrl = await _audioService.GenerateAudioFileAsync(
                    updated.Description, "vi", id, "vi"
                );

                if (audio != null)
                {
                    // Update existing audio
                    await _audioService.DeleteAudioFileAsync(audio.AudioUrl);
                    audio.TranscriptText = updated.Description;
                    audio.AudioUrl = newAudioUrl ?? audio.AudioUrl;
                }
                else
                {
                    // Tạo audio mới
                    _context.Audios.Add(new Audio
                    {
                        POIId = id,
                        LanguageCode = "vi",
                        TranscriptText = updated.Description,
                        AudioUrl = newAudioUrl ?? string.Empty
                    });
                }
            }

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

        // PUT /api/pois/{id}/localization -> thêm/cập nhật bản dịch (name + description) theo ngôn ngữ
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPut("{id}/localization")]
        public async Task<IActionResult> UpsertLocalization(int id, [FromBody] PoiLocalizationInput input)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound("POI not found");

            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi.VendorId != userId)
                return Forbid();

            var loc = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.POIId == id && l.LanguageCode == input.LanguageCode);

            if (loc == null)
            {
                loc = new PoiLocalization
                {
                    POIId = id,
                    LanguageCode = input.LanguageCode,
                    TranslatedName = input.TranslatedName,
                    TranslatedDescription = input.TranslatedDescription
                };
                _context.PoiLocalizations.Add(loc);
            }
            else
            {
                loc.TranslatedName = input.TranslatedName;
                loc.TranslatedDescription = input.TranslatedDescription;
            }

            await _context.SaveChangesAsync();
            return Ok(loc);
        }

        // DELETE /api/pois/{id}/localization/{lang} -> xóa bản dịch
        [Authorize(Roles = "Admin,Vendor")]
        [HttpDelete("{id}/localization/{lang}")]
        public async Task<IActionResult> DeleteLocalization(int id, string lang)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound();

            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi.VendorId != userId)
                return Forbid();

            var loc = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.POIId == id && l.LanguageCode == lang);
            if (loc == null) return NotFound();

            _context.PoiLocalizations.Remove(loc);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    // DTO cho input localization
    public class PoiLocalizationInput
    {
        public string LanguageCode { get; set; } = string.Empty;
        public string TranslatedName { get; set; } = string.Empty;
        public string? TranslatedDescription { get; set; }
    }
}
