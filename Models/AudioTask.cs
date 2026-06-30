using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourGuideApp.Models
{
    public class AudioTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int POIId { get; set; }

        [Required]
        [MaxLength(10)]
        public string TargetLanguage { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [ForeignKey("POIId")]
        public POI? POI { get; set; }
    }
}
