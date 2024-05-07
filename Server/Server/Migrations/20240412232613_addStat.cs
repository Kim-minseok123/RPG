using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalExp",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "Dex",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Exp",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Int",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Luk",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMp",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Str",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dex",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Exp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Int",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Luk",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxMp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Str",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "TotalExp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
