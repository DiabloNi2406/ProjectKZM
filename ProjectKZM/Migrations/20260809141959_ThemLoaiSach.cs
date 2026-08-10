using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class ThemLoaiSach : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaLoaiSach",
                table: "Sachs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoaiSachs",
                columns: table => new
                {
                    MaLoaiSach = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiSach = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiSachs", x => x.MaLoaiSach);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sachs_MaLoaiSach",
                table: "Sachs",
                column: "MaLoaiSach");

            migrationBuilder.AddForeignKey(
                name: "FK_Sachs_LoaiSachs_MaLoaiSach",
                table: "Sachs",
                column: "MaLoaiSach",
                principalTable: "LoaiSachs",
                principalColumn: "MaLoaiSach",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sachs_LoaiSachs_MaLoaiSach",
                table: "Sachs");

            migrationBuilder.DropTable(
                name: "LoaiSachs");

            migrationBuilder.DropIndex(
                name: "IX_Sachs_MaLoaiSach",
                table: "Sachs");

            migrationBuilder.DropColumn(
                name: "MaLoaiSach",
                table: "Sachs");
        }
    }
}
