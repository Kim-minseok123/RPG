using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addClear : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCleard",
                table: "Quest",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCleard",
                table: "Quest");
        }
    }
}
