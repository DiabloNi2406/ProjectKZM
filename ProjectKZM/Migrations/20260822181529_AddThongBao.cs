using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectKZM.Migrations
{
    /// <inheritdoc />
    public partial class AddThongBao : Migration
    {
        /// <inheritdoc />
        protected override void Up(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThongBaos",
                columns: table => new
                {
                    MaThongBao =
                        table.Column<int>(
                            type: "int",
                            nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    MaNguoiDung =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    TieuDe =
                        table.Column<string>(
                            type: "nvarchar(200)",
                            maxLength: 200,
                            nullable: false),

                    NoiDung =
                        table.Column<string>(
                            type: "nvarchar(500)",
                            maxLength: 500,
                            nullable: false),

                    DuongDan =
                        table.Column<string>(
                            type: "nvarchar(300)",
                            maxLength: 300,
                            nullable: true),

                    Loai =
                        table.Column<string>(
                            type: "nvarchar(50)",
                            maxLength: 50,
                            nullable: true),

                    MaDonHang =
                        table.Column<int>(
                            type: "int",
                            nullable: true),

                    DaDoc =
                        table.Column<bool>(
                            type: "bit",
                            nullable: false),

                    NgayTao =
                        table.Column<DateTime>(
                            type: "datetime2",
                            nullable: false)
                },

                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ThongBaos",
                        x => x.MaThongBao);

                    table.ForeignKey(
                        name:
                            "FK_ThongBaos_NguoiDungs_MaNguoiDung",

                        column:
                            x => x.MaNguoiDung,

                        principalTable:
                            "NguoiDungs",

                        principalColumn:
                            "MaNguoiDung",

                        onDelete:
                            ReferentialAction.Cascade);
                });


            migrationBuilder.CreateIndex(
                name:
                    "IX_ThongBaos_MaNguoiDung",

                table:
                    "ThongBaos",

                column:
                    "MaNguoiDung");
        }


        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThongBaos");
        }
    }
}