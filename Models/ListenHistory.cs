using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    // Ghi log mỗi lần một bản Audio được phát (để vẽ biểu đồ thống kê)
    public class ListenHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int POIId { get; set; }

        [ForeignKey("POIId")]
        [JsonIgnore]
        public POI? POI { get; set; }

        [MaxLength(10)]
        public string LanguageCode { get; set; } = "vi";

        // "gps" = tự động kích hoạt qua định vị, "qr" = quét QR thủ động
        [MaxLength(10)]
        public string Source { get; set; } = "gps";

        public DateTime ListenedAt { get; set; } = DateTime.UtcNow;
    }
}
