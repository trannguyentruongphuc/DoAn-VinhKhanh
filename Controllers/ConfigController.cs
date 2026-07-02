using Microsoft.AspNetCore.Mvc;

namespace TourGuideApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            // === CẤU HÌNH ỨNG DỤNG ===
            // Sửa các giá trị bên dưới, frontend sẽ tự động nhận

            // Số thiết bị đếm cho mỗi tab trình duyệt
            visitorsPerTab = 1,

            // Bán kính thông báo khi vào gần POI (mét)
            notifyDist = 100,

            // Thời gian chờ giữa 2 lần phát audio tự động (mili-giây)
            cooldownMs = 30000,

            // Số lịch sử nghe tối đa lưu trong trình duyệt
            historyLimit = 20,

            // Tên ứng dụng
            appName = "Vĩnh Khánh Tour Guide",

            // Phiên bản
            version = "1.0.0"
        });
    }
}
