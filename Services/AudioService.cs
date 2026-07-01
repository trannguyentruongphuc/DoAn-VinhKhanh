using Microsoft.AspNetCore.Mvc;
using TourGuideApp.Data;
using TourGuideApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TourGuideApp.Services
{
    public class AudioService
    {
        private readonly TourGuideContext _context;
        private readonly ILogger<AudioService> _logger;

        public AudioService(TourGuideContext context, ILogger<AudioService> logger)
        {
            _context = context;
            _logger = logger;
        }
    }
}
