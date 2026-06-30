using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TourGuideApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvancedGithubFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VendorId",
                table: "POIs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AudioTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POIId = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetLanguage = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AudioTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AudioTasks_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DatasetVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatasetVersions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PoiLocalizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POIId = table.Column<int>(type: "INTEGER", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TranslatedName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TranslatedDescription = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoiLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoiLocalizations_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StoreName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PoiOwnerRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    BusinessAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ProofOfOwnership = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoiOwnerRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PoiOwnerRegistrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    POIId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_POIs_POIId",
                        column: x => x.POIId,
                        principalTable: "POIs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 1,
                column: "TranscriptText",
                value: "");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 2,
                column: "TranscriptText",
                value: "");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 3,
                column: "TranscriptText",
                value: "");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 4,
                column: "TranscriptText",
                value: "");

            migrationBuilder.InsertData(
                table: "Audios",
                columns: new[] { "Id", "AudioUrl", "LanguageCode", "ListenCount", "POIId", "TranscriptText" },
                values: new object[,]
                {
                    { 5, "/audio/poi2_ko.mp3", "ko", 0, 2, "" },
                    { 6, "/audio/poi2_zh.mp3", "zh", 0, 2, "" },
                    { 7, "/audio/poi3_vi.mp3", "vi", 0, 3, "" },
                    { 8, "/audio/poi3_zh.mp3", "zh", 0, 3, "" },
                    { 9, "/audio/poi3_en.mp3", "en", 0, 3, "" },
                    { 10, "/audio/poi3_ko.mp3", "ko", 0, 3, "" }
                });

            migrationBuilder.UpdateData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 1,
                column: "VendorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 2,
                column: "VendorId",
                value: null);

            migrationBuilder.UpdateData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Priority", "VendorId" },
                values: new object[] { 3, null });

            migrationBuilder.InsertData(
                table: "POIs",
                columns: new[] { "Id", "Description", "Lat", "Lng", "Name", "Priority", "Radius", "VendorId" },
                values: new object[,]
                {
                    { 4, "Quán bánh flan nổi tiếng trên phố Vĩnh Khánh.", 10.76139, 106.70010000000001, "Bánh Flan Ngọc Nga", 3, 80.0, null },
                    { 5, "Quán ốc Oanh nổi tiếng trên phố Vĩnh Khánh.", 10.75902, 106.69718, "Ốc Oanh Vĩnh Khánh", 2, 80.0, null }
                });

            migrationBuilder.InsertData(
                table: "Audios",
                columns: new[] { "Id", "AudioUrl", "LanguageCode", "ListenCount", "POIId", "TranscriptText" },
                values: new object[,]
                {
                    { 11, "/audio/poi4_vi.mp3", "vi", 0, 4, "" },
                    { 12, "/audio/poi4_ko.mp3", "ko", 0, 4, "" },
                    { 13, "/audio/poi4_zh.mp3", "zh", 0, 4, "" },
                    { 14, "/audio/poi4_en.mp3", "en", 0, 4, "" },
                    { 15, "/audio/poi5_vi.mp3", "vi", 0, 5, "" },
                    { 16, "/audio/poi5_en.mp3", "en", 0, 5, "" },
                    { 17, "/audio/poi5_ko.mp3", "ko", 0, 5, "" },
                    { 18, "/audio/poi5_zh.mp3", "zh", 0, 5, "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_POIs_VendorId",
                table: "POIs",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_AudioTasks_POIId",
                table: "AudioTasks",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_PoiLocalizations_POIId",
                table: "PoiLocalizations",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_PoiOwnerRegistrations_UserId",
                table: "PoiOwnerRegistrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_POIId",
                table: "Reviews",
                column: "POIId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_POIs_Users_VendorId",
                table: "POIs",
                column: "VendorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_POIs_Users_VendorId",
                table: "POIs");

            migrationBuilder.DropTable(
                name: "AudioTasks");

            migrationBuilder.DropTable(
                name: "DatasetVersions");

            migrationBuilder.DropTable(
                name: "PoiLocalizations");

            migrationBuilder.DropTable(
                name: "PoiOwnerRegistrations");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_POIs_VendorId",
                table: "POIs");

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "POIs");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 1,
                column: "TranscriptText",
                value: "Chào mừng bạn đến với phố ẩm thực Vĩnh Khánh.");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 2,
                column: "TranscriptText",
                value: "Welcome to Vinh Khanh food street.");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 3,
                column: "TranscriptText",
                value: "Đây là quán ốc nổi tiếng, hoạt động từ năm 1990.");

            migrationBuilder.UpdateData(
                table: "Audios",
                keyColumn: "Id",
                keyValue: 4,
                column: "TranscriptText",
                value: "This is a famous snail restaurant, operating since 1990.");

            migrationBuilder.UpdateData(
                table: "POIs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Priority",
                value: 1);
        }
    }
}
