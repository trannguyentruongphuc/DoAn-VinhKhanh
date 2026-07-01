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
        private readonly TranslationService _translationService;

        public POIController(TourGuideContext context, TranslationService translationService)
        {
            _context = context;
            _translationService = translationService;
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

            // Tự động dịch POI sang các ngôn ngữ khác (en, ko, zh)
            _ = _translationService.AutoTranslatePOIAsync(poi);

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

            poi.Name = updated.Name;
            poi.Description = updated.Description;
            poi.Lat = updated.Lat;
            poi.Lng = updated.Lng;
            poi.Radius = updated.Radius;
            poi.Priority = updated.Priority;

            var descriptionChanged = poi.Description != updated.Description;

            // Tự động dịch lại nếu description thay đổi
            if (descriptionChanged)
            {
                _ = _translationService.AutoTranslatePOIAsync(poi);
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

        // POST /api/pois/{id}/auto-translate -> tự động dịch POI sang các ngôn ngữ khác
        [Authorize(Roles = "Admin,Vendor")]
        [HttpPost("{id}/auto-translate")]
        public async Task<IActionResult> AutoTranslate(int id)
        {
            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound("POI not found");

            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (role == "Vendor" && poi.VendorId != userId)
                return Forbid();

            await _translationService.AutoTranslatePOIAsync(poi);

            // Reload POI để trả về data mới
            var updatedPoi = await _context.POIs
                .Include(p => p.Localizations)
                .Include(p => p.Audios)
                .FirstOrDefaultAsync(p => p.Id == id);

            return Ok(new {
                message = "Đã bắt đầu dịch tự động",
                poi = updatedPoi
            });
        }

        // POST /api/pois/{id}/translate/{lang} -> dịch POI sang một ngôn ngữ cụ thể (dùng cho user app)
        [HttpPost("{id}/translate/{lang}")]
        public async Task<IActionResult> TranslateToLanguage(int id, string lang)
        {
            if (lang == "vi")
                return Ok(new { message = "Không cần dịch tiếng Việt" });

            var poi = await _context.POIs.FindAsync(id);
            if (poi == null) return NotFound("POI not found");

            // Kiểm tra đã có bản dịch chưa
            var existingLoc = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.POIId == id && l.LanguageCode == lang);

            if (existingLoc != null && !string.IsNullOrWhiteSpace(existingLoc.TranslatedDescription))
            {
                return Ok(new {
                    translatedName = existingLoc.TranslatedName,
                    translatedDescription = existingLoc.TranslatedDescription,
                    source = "cached"
                });
            }

            // Gọi dịch
            var translatedName = await _translationService.TranslateAsync(poi.Name, lang);
            var translatedDesc = !string.IsNullOrWhiteSpace(poi.Description)
                ? await _translationService.TranslateAsync(poi.Description, lang)
                : translatedName ?? poi.Name;

            // Lưu vào DB
            if (existingLoc != null)
            {
                existingLoc.TranslatedName = translatedName ?? existingLoc.TranslatedName;
                existingLoc.TranslatedDescription = translatedDesc ?? existingLoc.TranslatedDescription;
            }
            else
            {
                _context.PoiLocalizations.Add(new PoiLocalization
                {
                    POIId = id,
                    LanguageCode = lang,
                    TranslatedName = translatedName ?? poi.Name,
                    TranslatedDescription = translatedDesc
                });
            }
            await _context.SaveChangesAsync();

            return Ok(new {
                translatedName = translatedName ?? poi.Name,
                translatedDescription = translatedDesc,
                source = "translated"
            });
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
