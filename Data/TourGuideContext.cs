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
                    Priority = 1
                }
            );

            modelBuilder.Entity<Audio>().HasData(
                new Audio { Id = 1, POIId = 1, LanguageCode = "vi", AudioUrl = "/audio/poi1_vi.mp3", TranscriptText = "Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh." },
                new Audio { Id = 2, POIId = 1, LanguageCode = "en", AudioUrl = "/audio/poi1_en.mp3", TranscriptText = "Welcome to Vinh Khanh food street." },
                new Audio { Id = 3, POIId = 2, LanguageCode = "vi", AudioUrl = "/audio/poi2_vi.mp3", TranscriptText = "Đây là quán ốc nổi tiếng, hoạt động từ năm 1990." },
                new Audio { Id = 4, POIId = 2, LanguageCode = "en", AudioUrl = "/audio/poi2_en.mp3", TranscriptText = "This is a famous snail restaurant, operating since 1990." }
            );
        }
    }
}
