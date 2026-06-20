using Microsoft.EntityFrameworkCore;
using TourGuideApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext dùng SQLite
builder.Services.AddDbContext<TourGuideContext>(options =>
    options.UseSqlite("Data Source=TourGuide.db"));

// 2. Đăng ký Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Tránh lỗi vòng lặp JSON khi POI <-> Audio tham chiếu qua lại
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 3. Đăng ký Swagger (để test API tại /swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. CORS: cho phép Frontend (index.html/admin.html) gọi API từ trình duyệt
//    QUAN TRỌNG: Phải UseCors TRƯỚC UseAuthorization (xem mục FAQ trong Hướng dẫn.docx)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Tự động tạo DB + áp dụng migration khi khởi động (tiện cho đồ án, không cần chạy lệnh tay)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourGuideContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); // Cho phép phục vụ file tĩnh trong wwwroot (index.html, admin.html, audio, ...)

app.UseCors("AllowAll"); // <-- PHẢI đặt trước UseAuthorization

app.UseAuthorization();

app.MapControllers();

app.Run();
