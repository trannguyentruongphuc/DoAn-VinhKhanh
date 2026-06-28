using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/Analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public AnalyticsController(TourGuideContext context)
        {
            _context = context;
        }

        // GET /api/Analytics/top-pois -> top 5 POI có lượt nghe nhiều nhất (mọi ngôn ngữ cộng lại)
        // Dùng cho biểu đồ cột Chart.js bên admin.html
        [HttpGet("top-pois")]
        public async Task<IActionResult> GetTopPois()
        {
            var data = await _context.ListenHistories
                .Include(h => h.POI)
                .GroupBy(h => new { h.POIId, h.POI!.Name })
                .Select(g => new
                {
                    poiId = g.Key.POIId,
                    poiName = g.Key.Name,
                    listenCount = g.Count()
                })
                .OrderByDescending(x => x.listenCount)
                .Take(5)
                .ToListAsync();

            return Ok(data);
        }

    // POST /api/Analytics/increment/{poiId}?lang=vi -> Ghi nhận 1 lượt nghe
        [HttpPost("increment/{poiId}")]
        public async Task<IActionResult> LogListenHistory(int poiId, [FromQuery] string lang = "vi")
        {
            // 1. Kiểm tra xem quán ăn này có tồn tại trong Database không
            var poiExists = await _context.POIs.AnyAsync(p => p.Id == poiId);
            if (!poiExists) return NotFound(new { message = "Không tìm thấy địa điểm." });

            // 2. Tạo một dòng lịch sử mới (Ghi nhận Quán nào, Ngôn ngữ gì)
            var history = new ListenHistory
            {
                POIId = poiId,
                LanguageCode = lang
            };

            // 3. Ném vào bảng Lịch sử và Lưu lại
            _context.ListenHistories.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Đã lưu lịch sử nghe điểm {poiId} bằng tiếng {lang}" });
        }
  
    }
}
