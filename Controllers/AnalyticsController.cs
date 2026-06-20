using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;

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

        // GET /api/Analytics/by-language -> số lượt nghe theo từng ngôn ngữ (phụ trợ, dễ làm thêm 1 chart)
        [HttpGet("by-language")]
        public async Task<IActionResult> GetByLanguage()
        {
            var data = await _context.ListenHistories
                .GroupBy(h => h.LanguageCode)
                .Select(g => new { language = g.Key, listenCount = g.Count() })
                .ToListAsync();

            return Ok(data);
        }
    }
}
