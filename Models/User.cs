using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = "User";

        // Thông tin cửa hàng (dành cho Vendor)
        [MaxLength(200)]
        public string? StoreName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Trạng thái tài khoản (Admin có thể khóa/mở khóa)
        public bool IsActive { get; set; } = true;

        // Navigation property
        [JsonIgnore]
        public List<POI>? OwnedPOIs { get; set; }

        [JsonIgnore]
        public ICollection<Review>? Reviews { get; set; }

        [JsonIgnore]
        public ICollection<FavoritePoi>? FavoritePois { get; set; }

        [JsonIgnore]
        public List<PoiOwnerRegistration>? PoiOwnerRegistrations { get; set; }
    }
}
