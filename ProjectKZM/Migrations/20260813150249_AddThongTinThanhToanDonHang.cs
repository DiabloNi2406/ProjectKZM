using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class AddThongTinThanhToanDonHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_MaNguoiDung",
                table: "YeuThichs");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "DonHangs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_MaNguoiDung_MaSach",
                table: "YeuThichs",
                columns: new[] { "MaNguoiDung", "MaSach" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YeuThichs_MaNguoiDung_MaSach",
                table: "YeuThichs");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DonHangs");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_MaNguoiDung",
                table: "YeuThichs",
                column: "MaNguoiDung");
        }
    }
}
