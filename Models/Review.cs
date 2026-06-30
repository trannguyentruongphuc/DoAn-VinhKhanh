using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int POIId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 stars

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ForeignKey("POIId")]
        public POI? POI { get; set; }

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
