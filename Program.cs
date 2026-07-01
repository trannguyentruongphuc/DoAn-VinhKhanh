using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TourGuideApp.Data;
using TourGuideApp.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký DbContext dùng SQLite
builder.Services.AddDbContext<TourGuideContext>(options =>
    options.UseSqlite("Data Source=TourGuide.db"));

// 2. Đăng ký TranslationService cho dịch tự động
builder.Services.AddScoped<TranslationService>();

// 2. Đăng ký Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 3. Cấu hình JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsAVeryLongSecretKeyForTourGuideApp2024!@#$%^&*()";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TourGuideApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TourGuideApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// 4. Đăng ký Swagger (để test API tại /swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// 5. CORS: cho phép Frontend (index.html/admin.html) gọi API từ trình duyệt
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

// Tự động tạo DB + seed data khi chạy lần đầu (clone về là có sẵn data)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TourGuideContext>();
    db.Database.EnsureCreated();

    // Đảm bảo bảng ListenHistories tồn tại (đề phòng db đã có từ trước)
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ListenHistories (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            POIId INTEGER NOT NULL,
            LanguageCode TEXT NOT NULL,
            ListenedAt TEXT NOT NULL,
            FOREIGN KEY (POIId) REFERENCES POIs(Id) ON DELETE CASCADE
        );
    ");

    // Tạo tài khoản Admin mặc định nếu chưa có
    if (!db.Users.Any(u => u.Username == "admin"))
    {
        db.Users.Add(new TourGuideApp.Models.User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Email = "admin@vinhkhanh.com",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }

    // Tạo tài khoản Vendor mẫu nếu chưa có
    if (!db.Users.Any(u => u.Username == "vendor"))
    {
        db.Users.Add(new TourGuideApp.Models.User
        {
            Username = "vendor",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("vendor123"),
            Email = "vendor@vinhkhanh.com",
            Role = "Vendor",
            StoreName = "Quán Ốc Đào Vĩnh Khánh",
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TourGuideApp.Hubs.VisitorHub>("/visitorHub");

// Lắng nghe trên tất cả interfaces để các thiết bị khác trong mạng LAN truy cập được
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5555");

app.Run();
