using System.ComponentModel.DataAnnotations;

namespace TourGuideApp.Models
{
    public class DatasetVersion
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string VersionNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Description { get; set; }
        
        public bool IsActive { get; set; } = false;
    }
}
