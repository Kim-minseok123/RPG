using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class updateQuickSlot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skill_Player_PlayerDbId",
                table: "Skill");

            migrationBuilder.DropIndex(
                name: "IX_Skill_PlayerDbId",
                table: "Skill");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerDbId",
                table: "Skill",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Skill_PlayerDbId_TemplateId",
                table: "Skill",
                columns: new[] { "PlayerDbId", "TemplateId" });

            migrationBuilder.CreateTable(
                name: "QuickSlot",
                columns: table => new
                {
                    QuickSlotDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    Slot = table.Column<int>(nullable: false),
                    PlayerDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickSlot", x => x.QuickSlotDbId);
                    table.UniqueConstraint("AK_QuickSlot_PlayerDbId_Slot", x => new { x.PlayerDbId, x.Slot });
                    table.ForeignKey(
                        name: "FK_QuickSlot_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Skill_Player_PlayerDbId",
                table: "Skill",
                column: "PlayerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Skill_Player_PlayerDbId",
                table: "Skill");

            migrationBuilder.DropTable(
                name: "QuickSlot");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Skill_PlayerDbId_TemplateId",
                table: "Skill");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerDbId",
                table: "Skill",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_Skill_PlayerDbId",
                table: "Skill",
                column: "PlayerDbId");

            migrationBuilder.AddForeignKey(
                name: "FK_Skill_Player_PlayerDbId",
                table: "Skill",
                column: "PlayerDbId",
                principalTable: "Player",
                principalColumn: "PlayerDbId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
