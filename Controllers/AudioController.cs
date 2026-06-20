using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/Audio")]
    public class AudioController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public AudioController(TourGuideContext context)
        {
            _context = context;
        }

        // GET /api/Audio -> toàn bộ bản audio (mọi POI, mọi ngôn ngữ)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Audio>>> GetAll()
        {
            var audios = await _context.Audios.OrderBy(a => a.POIId).ToListAsync();
            return Ok(audios);
        }

        // GET /api/Audio/poi/{poiId} -> tất cả bản audio (mọi ngôn ngữ) của 1 POI
        [HttpGet("poi/{poiId}")]
        public async Task<ActionResult<IEnumerable<Audio>>> GetByPoi(int poiId)
        {
            var audios = await _context.Audios
                .Where(a => a.POIId == poiId)
                .ToListAsync();
            return Ok(audios);
        }

        // POST /api/Audio -> gán 1 bản audio (1 ngôn ngữ) cho 1 POI
        // Body mẫu: { "poiId": 1, "languageCode": "en", "audioUrl": "/audio/poi1_en.mp3", "transcriptText": "..." }
        [HttpPost]
        public async Task<ActionResult<Audio>> Create([FromBody] Audio audio)
        {
            var poiExists = await _context.POIs.AnyAsync(p => p.Id == audio.POIId);
            if (!poiExists)
            {
                return BadRequest(new { message = $"POIId {audio.POIId} không tồn tại." });
            }

            // Tránh trùng: 1 POI chỉ nên có 1 bản audio cho mỗi ngôn ngữ
            var duplicate = await _context.Audios
                .AnyAsync(a => a.POIId == audio.POIId && a.LanguageCode == audio.LanguageCode);
            if (duplicate)
            {
                return Conflict(new { message = $"POI {audio.POIId} đã có audio ngôn ngữ '{audio.LanguageCode}'. Hãy dùng PUT để sửa." });
            }

            _context.Audios.Add(audio);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByPoi), new { poiId = audio.POIId }, audio);
        }

        // PUT /api/Audio/{id} -> sửa link audio / transcript
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Audio updated)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            audio.AudioUrl = updated.AudioUrl;
            audio.TranscriptText = updated.TranscriptText;
            audio.LanguageCode = updated.LanguageCode;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/Audio/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var audio = await _context.Audios.FindAsync(id);
            if (audio == null) return NotFound();

            _context.Audios.Remove(audio);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
