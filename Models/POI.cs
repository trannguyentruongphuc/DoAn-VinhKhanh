using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TourGuideApp.Models
{
    // POI = Point of Interest (1 quán ăn / 1 địa điểm trên phố Vĩnh Khánh)
    public class POI
    {
        [Key]
        public int Id { get; set; }

        // Tên hiển thị mặc định (vẫn giữ lại để tương thích/đơn giản, có thể là tên tiếng Việt)
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        // Tọa độ GPS
        public double Lat { get; set; }
        public double Lng { get; set; }

        // Bán kính kích hoạt (mét)
        public double Radius { get; set; } = 100;

        // Độ ưu tiên: khi 2 POI gần nhau cùng kích hoạt, ưu tiên số lớn hơn
        public int Priority { get; set; } = 1;

        // Quan hệ 1-N: 1 POI có nhiều bản Audio (mỗi ngôn ngữ 1 file)
        // Lưu ý: KHÔNG đặt [JsonIgnore] ở đây — admin.html và index.html cần
        // nhận field "audios" trong JSON trả về để hiển thị danh sách audio đã gán
        // và để window.playPoiById() tìm đúng audio theo ngôn ngữ khi quét QR.
        // Chiều ngược lại (Audio.POI) vẫn giữ [JsonIgnore] để tránh JSON lặp vô hạn.
        public List<Audio> Audios { get; set; } = new List<Audio>();
    }
}
