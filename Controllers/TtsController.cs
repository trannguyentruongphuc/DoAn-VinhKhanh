using Microsoft.AspNetCore.Mvc;

namespace TourGuideApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TtsController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetTts([FromQuery] string text, [FromQuery] string lang = "vi")
        {
            if (string.IsNullOrWhiteSpace(text)) return BadRequest("Text is empty");
            if (text.Length > 500) text = text.Substring(0, 500);

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                
                var url = $"https://translate.google.com/translate_tts?ie=UTF-8&tl={lang}&client=tw-ob&q={Uri.EscapeDataString(text)}";
                
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Google TTS failed");
                }

                var audioBytes = await response.Content.ReadAsByteArrayAsync();
                return File(audioBytes, "audio/mpeg");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
