using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class addQuest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quest",
                columns: table => new
                {
                    QuestDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    IsFinish = table.Column<bool>(nullable: false),
                    QuestType = table.Column<int>(nullable: false),
                    PlayerDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quest", x => x.QuestDbId);
                    table.UniqueConstraint("AK_Quest_PlayerDbId_TemplateId", x => new { x.PlayerDbId, x.TemplateId });
                    table.ForeignKey(
                        name: "FK_Quest_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestGoal",
                columns: table => new
                {
                    QuestGoalDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnwerQuestDbId = table.Column<int>(nullable: false),
                    TemplateId = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestGoal", x => x.QuestGoalDbId);
                    table.ForeignKey(
                        name: "FK_QuestGoal_Quest_OnwerQuestDbId",
                        column: x => x.OnwerQuestDbId,
                        principalTable: "Quest",
                        principalColumn: "QuestDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestGoal_OnwerQuestDbId",
                table: "QuestGoal",
                column: "OnwerQuestDbId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestGoal");

            migrationBuilder.DropTable(
                name: "Quest");
        }
    }
}
