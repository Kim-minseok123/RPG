using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addStat3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attack",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "Defense",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMale",
                table: "Player",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttack",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinAttack",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatPoint",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Defense",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "IsMale",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxAttack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MinAttack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatPoint",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "Attack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
