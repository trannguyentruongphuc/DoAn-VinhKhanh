using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [Route("api/public/reviews")]
    [ApiController]
    public class PublicReviewController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public PublicReviewController(TourGuideContext context)
        {
            _context = context;
        }

        [HttpGet("poi/{poiId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetReviewsForPoi(int poiId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.POIId == poiId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => new
            {
                r.Id,
                r.UserId,
                userName = r.User != null ? r.User.Username : "Người dùng",
                r.Rating,
                r.Comment,
                r.CreatedAt
            }).ToList();
        }

        [HttpPost]
        [Authorize] // Requires login
        public async Task<ActionResult<Review>> PostReview(Review review)
        {
            review.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReviewsForPoi", new { poiId = review.POIId }, review);
        }
    }
}
