using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;
using TourGuideApp.Models;

namespace TourGuideApp.Services
{
    public class TranslationService
    {
        private readonly TourGuideContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslationService> _logger;

        private static readonly Dictionary<string, string> LanguageCodes = new()
        {
            { "en", "English" },
            { "ko", "Korean" },
            { "zh", "Chinese" }
        };

        public TranslationService(TourGuideContext context, IWebHostEnvironment env, ILogger<TranslationService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Dịch một đoạn text từ tiếng Việt sang ngôn ngữ khác
        /// Sử dụng Google Translate API miễn phí
        /// </summary>
        public async Task<string?> TranslateAsync(string text, string targetLang)
        {
            if (string.IsNullOrWhiteSpace(text) || targetLang == "vi")
                return text;

            try
            {
                // Sử dụng Google Translate API (translate.googleapis.com)
                // Đây là endpoint công khai, không cần API key
                var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=vi&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Translation API failed with status: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();

                // Parse JSON response từ Google Translate
                // Format: [[translated_text, original_text, confidence], ...]
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var firstElement = root[0];
                    if (firstElement.ValueKind == JsonValueKind.Array && firstElement.GetArrayLength() > 0)
                    {
                        var translatedText = new StringBuilder();
                        foreach (var segment in firstElement.EnumerateArray())
                        {
                            if (segment.ValueKind == JsonValueKind.Array && segment.GetArrayLength() > 0)
                            {
                                translatedText.Append(segment[0].GetString());
                            }
                        }
                        var result = translatedText.ToString();
                        _logger.LogInformation($"Translated '{text.Substring(0, Math.Min(30, text.Length))}...' to {targetLang}: '{result.Substring(0, Math.Min(30, result.Length))}...'");
                        return result;
                    }
                }

                _logger.LogWarning($"Unexpected translation response format: {json}");
                return null;
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Translation request timed out");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error translating text to {targetLang}");
                return null;
            }
        }

        /// <summary>
        /// Tự động tạo bản dịch cho tất cả ngôn ngữ (en, ko, zh) từ tiếng Việt
        /// </summary>
        public async Task AutoTranslatePOIAsync(POI poi)
        {
            _logger.LogInformation($"Starting auto-translation for POI ID: {poi.Id} - {poi.Name}");

            foreach (var lang in LanguageCodes.Keys)
            {
                try
                {
                    // Check nếu đã có bản dịch rồi thì skip
                    var existingLoc = await _context.PoiLocalizations
                        .FirstOrDefaultAsync(l => l.POIId == poi.Id && l.LanguageCode == lang);

                    if (existingLoc != null && !string.IsNullOrWhiteSpace(existingLoc.TranslatedName))
                    {
                        _logger.LogInformation($"Skip {lang} - translation already exists for POI {poi.Id}");
                        continue;
                    }

                    // Translate name và description
                    var translatedName = await TranslateAsync(poi.Name, lang);
                    var translatedDesc = !string.IsNullOrWhiteSpace(poi.Description)
                        ? await TranslateAsync(poi.Description, lang)
                        : null;

                    if (existingLoc != null)
                    {
                        // Update bản dịch đã tồn tại
                        existingLoc.TranslatedName = translatedName ?? existingLoc.TranslatedName;
                        existingLoc.TranslatedDescription = translatedDesc ?? existingLoc.TranslatedDescription;
                        _logger.LogInformation($"Updated {lang} translation for POI {poi.Id}");
                    }
                    else
                    {
                        // Tạo bản dịch mới
                        _context.PoiLocalizations.Add(new PoiLocalization
                        {
                            POIId = poi.Id,
                            LanguageCode = lang,
                            TranslatedName = translatedName ?? poi.Name,
                            TranslatedDescription = translatedDesc
                        });
                        _logger.LogInformation($"Created {lang} translation for POI {poi.Id}");
                    }

                    await _context.SaveChangesAsync();

                    // Delay để tránh rate limit
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error translating POI {poi.Id} to {lang}");
                }
            }

            _logger.LogInformation($"Completed auto-translation for POI ID: {poi.Id}");
        }

        /// <summary>
        /// Dịch một POI cụ thể sang một ngôn ngữ
        /// </summary>
        public async Task<PoiLocalization?> TranslatePOIToLanguageAsync(int poiId, string targetLang)
        {
            if (targetLang == "vi")
                return null;

            var poi = await _context.POIs.FindAsync(poiId);
            if (poi == null)
                return null;

            var translatedName = await TranslateAsync(poi.Name, targetLang);
            var translatedDesc = !string.IsNullOrWhiteSpace(poi.Description)
                ? await TranslateAsync(poi.Description, targetLang)
                : null;

            var existingLoc = await _context.PoiLocalizations
                .FirstOrDefaultAsync(l => l.POIId == poiId && l.LanguageCode == targetLang);

            if (existingLoc != null)
            {
                existingLoc.TranslatedName = translatedName ?? existingLoc.TranslatedName;
                existingLoc.TranslatedDescription = translatedDesc ?? existingLoc.TranslatedDescription;
            }
            else
            {
                existingLoc = new PoiLocalization
                {
                    POIId = poiId,
                    LanguageCode = targetLang,
                    TranslatedName = translatedName ?? poi.Name,
                    TranslatedDescription = translatedDesc
                };
                _context.PoiLocalizations.Add(existingLoc);
            }

            await _context.SaveChangesAsync();
            return existingLoc;
        }

        /// <summary>
        /// Kiểm tra xem POI đã có bản dịch cho ngôn ngữ chưa
        /// </summary>
        public async Task<bool> HasTranslationAsync(int poiId, string lang)
        {
            if (lang == "vi")
                return true;

            return await _context.PoiLocalizations
                .AnyAsync(l => l.POIId == poiId && l.LanguageCode == lang);
        }

        /// <summary>
        /// Lấy danh sách ngôn ngữ chưa được dịch cho một POI
        /// </summary>
        public async Task<List<string>> GetMissingTranslationsAsync(int poiId)
        {
            var missing = new List<string>();
            foreach (var lang in LanguageCodes.Keys)
            {
                if (!await HasTranslationAsync(poiId, lang))
                {
                    missing.Add(lang);
                }
            }
            return missing;
        }
    }
}
