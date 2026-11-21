using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BelediyeBasvuru.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class HesapYonetimi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PasifMi",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DenetimKayitlari",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZamanUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    YapanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    HedefId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Ip = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Islem = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EskiDeger = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    YeniDeger = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DenetimKayitlari", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DenetimKayitlari");

            migrationBuilder.DropColumn(
                name: "PasifMi",
                table: "AspNetUsers");
        }
    }
}
