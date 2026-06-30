using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class PoiLocalization
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int POIId { get; set; }

        [Required]
        [MaxLength(10)]
        public string LanguageCode { get; set; } = string.Empty; // e.g., "en", "vi", "ko", "zh"

        [Required]
        [MaxLength(255)]
        public string TranslatedName { get; set; } = string.Empty;

        public string? TranslatedDescription { get; set; }

        [JsonIgnore]
        [ForeignKey("POIId")]
        public POI? POI { get; set; }
    }
}
