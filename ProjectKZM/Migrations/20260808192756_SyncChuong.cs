using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class SyncChuong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chuong đã được tạo trong migration ThemTruongMoi.
            // Không thêm lại cột Chuong ở đây.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Không làm gì.
        }
    }
}