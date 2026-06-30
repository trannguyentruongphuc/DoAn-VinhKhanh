using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public FavoritesController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetFavorites()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var favorites = await _context.FavoritePois
                .Where(f => f.UserId == userId)
                .Include(f => f.POI)
                .Select(f => new {
                    f.Id,
                    f.CreatedAt,
                    PoiId = f.POI!.Id,
                    PoiName = f.POI!.Name,
                    PoiDescription = f.POI!.Description,
                    PoiLat = f.POI!.Lat,
                    PoiLng = f.POI!.Lng
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpGet("check/{poiId}")]
        public async Task<ActionResult<bool>> CheckFavorite(int poiId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var exists = await _context.FavoritePois.AnyAsync(f => f.UserId == userId && f.POIId == poiId);
            return Ok(exists);
        }

        [HttpPost("{poiId}")]
        public async Task<IActionResult> ToggleFavorite(int poiId)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

            var existingFav = await _context.FavoritePois
                .FirstOrDefaultAsync(f => f.UserId == userId && f.POIId == poiId);

            if (existingFav != null)
            {
                // Remove favorite
                _context.FavoritePois.Remove(existingFav);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã bỏ yêu thích", status = "removed" });
            }
            else
            {
                // Add favorite
                var fav = new FavoritePoi
                {
                    UserId = userId,
                    POIId = poiId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.FavoritePois.Add(fav);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã lưu vào danh sách yêu thích", status = "added" });
            }
        }
    }
}
