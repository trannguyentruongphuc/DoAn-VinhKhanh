using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    // Audio = 1 bản thuyết minh (ứng với 1 POI và 1 ngôn ngữ cụ thể)
    public class Audio
    {
        [Key]
        public int Id { get; set; }

        // Khóa ngoại liên kết tới POI
        [Required]
        public int POIId { get; set; }

        [ForeignKey("POIId")]
        [JsonIgnore]
        public POI? POI { get; set; }

        // Mã ngôn ngữ: "vi" | "en" | "ko" | "zh" ... (đa ngôn ngữ nằm ở đây)
        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } = "vi";

        // Link file mp3 (có thể là Google Drive link, hoặc file local trong wwwroot/audio)
        [MaxLength(500)]
        public string AudioUrl { get; set; } = string.Empty;

        // Văn bản thuyết minh tương ứng ngôn ngữ này (dùng để hiển thị + làm phụ đề)
        [MaxLength(2000)]
        public string TranscriptText { get; set; } = string.Empty;

        // Đếm số lượt nghe -> phục vụ Module Analytics (Top 5 địa điểm nghe nhiều nhất)
        public int ListenCount { get; set; } = 0;
    }
}
