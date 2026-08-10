using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class TaoGioHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs");

            migrationBuilder.InsertData(
                table: "VaiTros",
                columns: new[] { "MaVaiTro", "TenVaiTro" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs",
                column: "MaVaiTro",
                principalTable: "VaiTros",
                principalColumn: "MaVaiTro",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs");

            migrationBuilder.DeleteData(
                table: "VaiTros",
                keyColumn: "MaVaiTro",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "VaiTros",
                keyColumn: "MaVaiTro",
                keyValue: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_NguoiDungs_VaiTros_MaVaiTro",
                table: "NguoiDungs",
                column: "MaVaiTro",
                principalTable: "VaiTros",
                principalColumn: "MaVaiTro",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
