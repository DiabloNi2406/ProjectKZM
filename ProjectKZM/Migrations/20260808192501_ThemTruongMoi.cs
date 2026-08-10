using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class ThemTruongMoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Chuong",
                table: "Sachs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chuong",
                table: "Sachs");
        }
    }
}