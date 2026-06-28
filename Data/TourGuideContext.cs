using Microsoft.EntityFrameworkCore;
using TourGuideApp.Models;

namespace TourGuideApp.Data
{
    public class TourGuideContext : DbContext
    {
        public TourGuideContext(DbContextOptions<TourGuideContext> options) : base(options) { }

        public DbSet<POI> POIs { get; set; } = null!;
        public DbSet<Audio> Audios { get; set; } = null!;
        public DbSet<ListenHistory> ListenHistories { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Quan hệ 1-N: 1 POI có nhiều Audio (mỗi ngôn ngữ 1 bản)
            modelBuilder.Entity<Audio>()
                .HasOne(a => a.POI)
                .WithMany(p => p.Audios)
                .HasForeignKey(a => a.POIId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa POI thì xóa luôn Audio liên quan

            modelBuilder.Entity<ListenHistory>()
                .HasOne(h => h.POI)
                .WithMany()
                .HasForeignKey(h => h.POIId)
                .OnDelete(DeleteBehavior.Cascade);



            // Quan hệ Vendor - POI: 1 Vendor có thể sở hữu nhiều POI
            modelBuilder.Entity<POI>()
                .HasOne(p => p.Vendor)
                .WithMany()
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- SEED DATA: dữ liệu mẫu cho phố ẩm thực Vĩnh Khánh (Q4, TP.HCM) ---
            modelBuilder.Entity<POI>().HasData(
                new POI
                {
                    Id = 1,
                    Name = "Cổng phố ẩm thực Vĩnh Khánh",
                    Description = "Điểm khởi đầu tuyến phố ẩm thực Vĩnh Khánh, Quận 4.",
                    Lat = 10.7600,
                    Lng = 106.6960,
                    Radius = 100,
                    Priority = 1
                },
                new POI
                {
                    Id = 2,
                    Name = "Ốc Đào Vĩnh Khánh",
                    Description = "Quán ốc nổi tiếng lâu đời trên phố Vĩnh Khánh.",
                    Lat = 10.7595,
                    Lng = 106.6965,
                    Radius = 80,
                    Priority = 2
                },
                new POI
                {
                    Id = 3,
                    Name = "Bánh tráng trộn Vĩnh Khánh",
                    Description = "Khu vực tập trung các xe bánh tráng trộn về đêm.",
                    Lat = 10.7590,
                    Lng = 106.6970,
                    Radius = 80,
                    Priority = 3
                },
                new POI
                {
                    Id = 4,
                    Name = "Bánh Flan Ngọc Nga",
                    Description = "Quán bánh flan nổi tiếng trên phố Vĩnh Khánh.",
                    Lat = 10.76139,
                    Lng = 106.70010,
                    Radius = 80,
                    Priority = 3
                },
                new POI
                {
                    Id = 5,
                    Name = "Ốc Oanh Vĩnh Khánh",
                    Description = "Quán ốc Oanh nổi tiếng trên phố Vĩnh Khánh.",
                    Lat = 10.75902,
                    Lng = 106.69718,
                    Radius = 80,
                    Priority = 2
                }
            );

            modelBuilder.Entity<Audio>().HasData(
                // --- Audio cho POI 1: Cổng phố ẩm thực Vĩnh Khánh ---
                new Audio { Id = 1, POIId = 1, LanguageCode = "vi", AudioUrl = "/audio/poi1_vi.mp3", TranscriptText = "" },
                new Audio { Id = 2, POIId = 1, LanguageCode = "en", AudioUrl = "/audio/poi1_en.mp3", TranscriptText = "" },

                // --- Audio cho POI 2: Ốc Đào Vĩnh Khánh ---
                new Audio { Id = 3, POIId = 2, LanguageCode = "vi", AudioUrl = "/audio/poi2_vi.mp3", TranscriptText = "" },
                new Audio { Id = 4, POIId = 2, LanguageCode = "en", AudioUrl = "/audio/poi2_en.mp3", TranscriptText = "" },
                new Audio { Id = 5, POIId = 2, LanguageCode = "ko", AudioUrl = "/audio/poi2_ko.mp3", TranscriptText = "" },
                new Audio { Id = 6, POIId = 2, LanguageCode = "zh", AudioUrl = "/audio/poi2_zh.mp3", TranscriptText = "" },

                // --- Audio cho POI 3: Bánh tráng trộn Vĩnh Khánh ---
                new Audio { Id = 7, POIId = 3, LanguageCode = "vi", AudioUrl = "/audio/poi3_vi.mp3", TranscriptText = "" },
                new Audio { Id = 8, POIId = 3, LanguageCode = "zh", AudioUrl = "/audio/poi3_zh.mp3", TranscriptText = "" },
                new Audio { Id = 9, POIId = 3, LanguageCode = "en", AudioUrl = "/audio/poi3_en.mp3", TranscriptText = "" },
                new Audio { Id = 10, POIId = 3, LanguageCode = "ko", AudioUrl = "/audio/poi3_ko.mp3", TranscriptText = "" },

                // --- Audio cho POI 4: Bánh Flan Ngọc Nga ---
                new Audio { Id = 11, POIId = 4, LanguageCode = "vi", AudioUrl = "/audio/poi4_vi.mp3", TranscriptText = "" },
                new Audio { Id = 12, POIId = 4, LanguageCode = "ko", AudioUrl = "/audio/poi4_ko.mp3", TranscriptText = "" },
                new Audio { Id = 13, POIId = 4, LanguageCode = "zh", AudioUrl = "/audio/poi4_zh.mp3", TranscriptText = "" },
                new Audio { Id = 14, POIId = 4, LanguageCode = "en", AudioUrl = "/audio/poi4_en.mp3", TranscriptText = "" },

                // --- Audio cho POI 5: Ố Oanh Vĩnh Khánh ---
                new Audio { Id = 15, POIId = 5, LanguageCode = "vi", AudioUrl = "/audio/poi5_vi.mp3", TranscriptText = "" },
                new Audio { Id = 16, POIId = 5, LanguageCode = "en", AudioUrl = "/audio/poi5_en.mp3", TranscriptText = "" },
                new Audio { Id = 17, POIId = 5, LanguageCode = "ko", AudioUrl = "/audio/poi5_ko.mp3", TranscriptText = "" },
                new Audio { Id = 18, POIId = 5, LanguageCode = "zh", AudioUrl = "/audio/poi5_zh.mp3", TranscriptText = "" }
            );

            // --- SEED DATA: Tài khoản Admin mặc định ---
            // Admin account được tạo trong Program.cs bằng BCrypt
        }
    }
}
