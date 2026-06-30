using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class FavoritePoi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int POIId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [JsonIgnore]
        [ForeignKey("POIId")]
        public POI? POI { get; set; }
    }
}
