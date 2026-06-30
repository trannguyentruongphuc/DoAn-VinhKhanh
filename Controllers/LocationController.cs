using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Helpers;
using TourGuideApp.Models;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/Location")]
    public class LocationController : ControllerBase
    {
        private readonly TourGuideContext _context;

        public LocationController(TourGuideContext context)
        {
            _context = context;
        }

        // GET /api/location/check?lat=...&lng=...&lang=vi
        // lang: mã ngôn ngữ người dùng đang chọn trên giao diện ("vi" mặc định)
        [HttpGet("check")]
        public async Task<IActionResult> Check(double lat, double lng, string lang = "vi")
        {
            // .Include(p => p.Audios, p => p.Localizations): nạp kèm toàn bộ bản audio và bản dịch của từng POI
            var pois = await _context.POIs
                .Include(p => p.Audios)
                .Include(p => p.Localizations)
                .ToListAsync();

            // Lọc các điểm mà người dùng đang nằm trong vùng bán kính
            var candidates = pois
                .Select(p => new
                {
                    Poi = p,
                    Distance = GeoHelper.CalculateDistanceMeters(lat, lng, p.Lat, p.Lng)
                })
                .Where(x => x.Distance <= x.Poi.Radius)
                .ToList();

            if (!candidates.Any())
            {
                return Ok(new { triggered = false });
            }

            // Nếu có nhiều điểm cùng kích hoạt -> chọn điểm có Priority cao nhất,
            // nếu bằng nhau thì chọn điểm gần nhất
            var best = candidates
                .OrderByDescending(x => x.Poi.Priority)
                .ThenBy(x => x.Distance)
                .First()
                .Poi;

            // Tìm bản dịch theo ngôn ngữ
            var loc = best.Localizations?.FirstOrDefault(l => l.LanguageCode == lang);
            var translatedName = loc?.TranslatedName ?? best.Name;
            var translatedDesc = loc?.TranslatedDescription ?? best.Description;

            // Tìm audio theo ngôn ngữ, fallback về vi nếu không có
            var audio = best.Audios.FirstOrDefault(a => a.LanguageCode == lang)
                        ?? best.Audios.FirstOrDefault(a => a.LanguageCode == "vi")
                        ?? best.Audios.FirstOrDefault();

            // Ghi log lượt nghe phục vụ thống kê (nguồn: gps)
            if (audio != null)
            {
                _context.ListenHistories.Add(new ListenHistory
                {
                    POIId = best.Id,
                    LanguageCode = audio.LanguageCode,
                    Source = "gps"
                });
                audio.ListenCount += 1;
                await _context.SaveChangesAsync();
            }

            // Nếu người dùng chọn ngôn ngữ KHÔNG PHẢI vi mà không có audio → trả null để frontend dùng TTS
            var audioUrl = (lang != "vi" && best.Audios.All(a => a.LanguageCode != lang) && best.Audios.Any(a => a.LanguageCode == "vi"))
                           ? null : audio?.AudioUrl;
            var transcriptText = (lang != "vi" && best.Audios.All(a => a.LanguageCode != lang) && best.Audios.Any(a => a.LanguageCode == "vi"))
                                 ? null : audio?.TranscriptText;

            return Ok(new
            {
                triggered = true,
                poi = new
                {
                    id = best.Id,
                    name = best.Name,
                    description = best.Description,
                    lat = best.Lat,
                    lng = best.Lng,
                    radius = best.Radius,
                    priority = best.Priority,
                    localizations = best.Localizations?.Select(l => new { l.LanguageCode, l.TranslatedName, l.TranslatedDescription }),
                    audios = best.Audios?.Select(a => new { a.LanguageCode, a.AudioUrl, a.TranscriptText })
                },
                translatedName,
                translatedDesc,
                audioUrl,
                transcriptText,
                languageCode = audio?.LanguageCode
            });
        }
    }
}
