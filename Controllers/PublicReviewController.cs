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
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsForPoi(int poiId)
        {
            return await _context.Reviews
                .Where(r => r.POIId == poiId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
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
