using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class PoiOwnerRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string BusinessAddress { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? ProofOfOwnership { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
