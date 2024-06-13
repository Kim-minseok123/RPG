using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addSkillpoint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkillPoint",
                table: "Player",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkillPoint",
                table: "Player");
        }
    }
}
