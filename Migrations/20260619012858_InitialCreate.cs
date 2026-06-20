using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TourGuideApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Lat = table.Column<double>(type: "REAL", nullable: false),
                    Lng = table.Column<double>(type: "REAL", nullable: false),
                    Radius = table.Column<double>(type: "REAL", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Audios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POIId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    AudioUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TranscriptText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ListenCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audios_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListenHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POIId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    ListenedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListenHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ListenHistories_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "POIs",
                columns: new[] { "Id", "Description", "Lat", "Lng", "Name", "Priority", "Radius" },
                values: new object[,]
                {
                    { 1, "Điểm khởi đầu tuyến phố ẩm thực Vĩnh Khánh, Quận 4.", 10.76, 106.696, "Cổng phố ẩm thực Vĩnh Khánh", 1, 100.0 },
                    { 2, "Quán ốc nổi tiếng lâu đời trên phố Vĩnh Khánh.", 10.759499999999999, 106.6965, "Ốc Đào Vĩnh Khánh", 2, 80.0 },
                    { 3, "Khu vực tập trung các xe bánh tráng trộn về đêm.", 10.759, 106.697, "Bánh tráng trộn Vĩnh Khánh", 1, 80.0 }
                });

            migrationBuilder.InsertData(
                table: "Audios",
                columns: new[] { "Id", "AudioUrl", "LanguageCode", "ListenCount", "POIId", "TranscriptText" },
                values: new object[,]
                {
                    { 1, "/audio/poi1_vi.mp3", "vi", 0, 1, "Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh." },
                    { 2, "/audio/poi1_en.mp3", "en", 0, 1, "Welcome to Vinh Khanh food street." },
                    { 3, "/audio/poi2_vi.mp3", "vi", 0, 2, "Đây là quán ốc nổi tiếng, hoạt động từ năm 1990." },
                    { 4, "/audio/poi2_en.mp3", "en", 0, 2, "This is a famous snail restaurant, operating since 1990." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audios_POIId",
                table: "Audios",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_ListenHistories_POIId",
                table: "ListenHistories",
                column: "POIId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audios");

            migrationBuilder.DropTable(
                name: "ListenHistories");

            migrationBuilder.DropTable(
                name: "POIs");
        }
    }
}
