using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/listenhistory")]
    public class ListenHistoryController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public ListenHistoryController(TourGuideContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        // GET /api/listenhistory -> toàn bộ lịch sử nghe (Admin)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ListenHistory>>> GetAll()
        {
            var history = await _context.ListenHistories
                .OrderByDescending(h => h.ListenedAt)
                .Take(1000)
                .ToListAsync();
            return Ok(history);
        }

        // GET /api/listenhistory/my -> lịch sử nghe của Vendor (chỉ POI của mình)
        [Authorize(Roles = "Vendor")]
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ListenHistory>>> GetMyStats()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Get all POIs owned by this vendor
            var vendorPoiIds = await _context.POIs
                .Where(p => p.VendorId == userId)
                .Select(p => p.Id)
                .ToListAsync();

            var history = await _context.ListenHistories
                .Where(h => vendorPoiIds.Contains(h.POIId))
                .OrderByDescending(h => h.ListenedAt)
                .Take(1000)
                .ToListAsync();
            return Ok(history);
        }

        // GET /api/listenhistory/poi/{poiId} -> lịch sử nghe theo POI
        [HttpGet("poi/{poiId}")]
        public async Task<ActionResult<IEnumerable<ListenHistory>>> GetByPoi(int poiId)
        {
            var history = await _context.ListenHistories
                .Where(h => h.POIId == poiId)
                .OrderByDescending(h => h.ListenedAt)
                .ToListAsync();
            return Ok(history);
        }

        // POST /api/listenhistory -> ghi nhận lượt nghe
        [HttpPost]
        public async Task<ActionResult<ListenHistory>> Create([FromBody] ListenHistory history)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            history.ListenedAt = DateTime.UtcNow;
            _context.ListenHistories.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByPoi), new { poiId = history.POIId }, history);
        }

        // GET /api/listenhistory/stats -> thống kê (Admin)
        [Authorize(Roles = "Admin,Vendor")]
        [HttpGet("stats")]
        public async Task<ActionResult> GetStats()
        {
            var userId = GetCurrentUserId();
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<ListenHistory> query = _context.ListenHistories;

            // Vendor chỉ thấy POI của mình
            if (role == "Vendor" && userId != null)
            {
                var vendorPoiIds = await _context.POIs
                    .Where(p => p.VendorId == userId)
                    .Select(p => p.Id)
                    .ToListAsync();
                query = query.Where(h => vendorPoiIds.Contains(h.POIId));
            }

            var allListens = await query.ToListAsync();
            var now = DateTime.UtcNow;
            var thisMonth = allListens.Where(h => h.ListenedAt >= new DateTime(now.Year, now.Month, 1)).ToList();

            // Stats by language
            var langStats = allListens
                .GroupBy(h => h.LanguageCode ?? "unknown")
                .Select(g => new { language = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();

            // Stats by POI
            var poiStats = allListens
                .GroupBy(h => h.POIId)
                .Select(g => new { poiId = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .ToList();

            // Daily stats for last 7 days
            var sevenDaysAgo = now.AddDays(-6).Date;
            var dailyStats = Enumerable.Range(0, 7)
                .Select(i => {
                    var date = sevenDaysAgo.AddDays(i).Date;
                    var count = allListens.Count(h => h.ListenedAt.Date == date);
                    return new { date = date.ToString("yyyy-MM-dd"), count };
                })
                .ToList();

            return Ok(new
            {
                totalListens = allListens.Count,
                monthlyListens = thisMonth.Count,
                byLanguage = langStats,
                byPOI = poiStats,
                daily = dailyStats
            });
        }
    }
}
