using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BelediyeBasvuru.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BasvuruModelGuncelleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdSoyad",
                table: "Basvurular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Basvurular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Konum",
                table: "Basvurular",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Basvurular",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdSoyad",
                table: "Basvurular");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Basvurular");

            migrationBuilder.DropColumn(
                name: "Konum",
                table: "Basvurular");

            migrationBuilder.DropColumn(
                name: "Telefon",
                table: "Basvurular");
        }
    }
}
