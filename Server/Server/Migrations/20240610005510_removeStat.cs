using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class removeStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxAttack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MinAttack",
                table: "Player");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxAttack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinAttack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
