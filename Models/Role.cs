using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [JsonIgnore]
        public List<User>? Users { get; set; }
    }
}
