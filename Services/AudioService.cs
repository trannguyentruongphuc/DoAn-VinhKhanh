using Microsoft.AspNetCore.Mvc;
using TourGuideApp.Data;
using TourGuideApp.Models;
using System.Net.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace TourGuideApp.Services
{
    public class AudioService
    {
        private readonly TourGuideContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AudioService> _logger;

        public AudioService(TourGuideContext context, IWebHostEnvironment env, ILogger<AudioService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        /// <summary>
        /// Generate audio file từ text sử dụng Google TTS, lưu vào wwwroot/audio/
        /// </summary>
        public async Task<string?> GenerateAudioFileAsync(string text, string lang, int poiId, string languageCode)
        {
            try
            {
                // Tạo thư mục audio nếu chưa có
                var audioDir = Path.Combine(_env.ContentRootPath, "wwwroot", "audio");
                if (!Directory.Exists(audioDir))
                {
                    Directory.CreateDirectory(audioDir);
                }

                // Tạo filename duy nhất: poi_{id}_{lang}_{timestamp}.mp3
                var fileName = $"poi_{poiId}_{languageCode}_{DateTime.UtcNow.Ticks}.mp3";
                var filePath = Path.Combine(audioDir, fileName);

                // Gọi Google TTS API
                var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={lang}&client=tw-ob&q={Uri.EscapeDataString(text)}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Google TTS API failed with status: {response.StatusCode}");
                    return null;
                }

                // Lưu file audio
                var audioBytes = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(filePath, audioBytes);

                // Trả về URL để lưu vào DB
                var audioUrl = $"/audio/{fileName}";
                _logger.LogInformation($"Generated audio: {audioUrl}");
                return audioUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio file");
                return null;
            }
        }

        /// <summary>
        /// Xóa file audio vật lý khi không cần nữa
        /// </summary>
        public async Task DeleteAudioFileAsync(string? audioUrl)
        {
            if (string.IsNullOrEmpty(audioUrl) || !audioUrl.StartsWith("/audio/"))
                return;

            try
            {
                var fileName = audioUrl.Replace("/audio/", "");
                var filePath = Path.Combine(_env.ContentRootPath, "wwwroot", "audio", fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                    _logger.LogInformation($"Deleted audio file: {fileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting audio file: {audioUrl}");
            }
        }

        /// <summary>
        /// Tự động generate audio khi vendor cập nhật transcript
        /// </summary>
        public async Task<(string? audioUrl, string? transcriptText)> AutoGenerateAudioAsync(int poiId, string languageCode, string? newTranscript, string? providedAudioUrl)
        {
            // Nếu vendor cung cấp audio URL cụ thể (từ Google Drive, hosting khác...) → dùng luôn
            if (!string.IsNullOrEmpty(providedAudioUrl))
            {
                return (providedAudioUrl, newTranscript ?? string.Empty);
            }

            // Nếu có transcript text → tự generate audio từ TTS
            if (!string.IsNullOrWhiteSpace(newTranscript))
            {
                var generatedUrl = await GenerateAudioFileAsync(newTranscript, languageCode, poiId, languageCode);
                if (generatedUrl != null)
                {
                    return (generatedUrl, newTranscript);
                }
            }

            // Không có gì → trả về rỗng
            return (null, newTranscript ?? string.Empty);
        }
    }
}
