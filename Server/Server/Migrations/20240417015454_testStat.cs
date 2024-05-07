using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class testStat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Defense",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Dex",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Exp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Hp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Int",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Luk",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxAttack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxHp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MaxMp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "MinAttack",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Mp",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Speed",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "StatPoint",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Str",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "posX",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "posY",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "posZ",
                table: "Player",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "rotateY",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "posX",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "posY",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "posZ",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "rotateY",
                table: "Player");

            migrationBuilder.AddColumn<int>(
                name: "Defense",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dex",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Exp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Int",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Luk",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAttack",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxHp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMp",
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

            migrationBuilder.AddColumn<int>(
                name: "Mp",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "Speed",
                table: "Player",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "StatPoint",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Str",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
