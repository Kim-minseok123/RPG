using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class Inits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountLoginId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountDbId);
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    PlayerDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerName = table.Column<string>(nullable: true),
                    IsMale = table.Column<bool>(nullable: false),
                    AccountDbId = table.Column<int>(nullable: false),
                    PlayerClass = table.Column<int>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Hp = table.Column<int>(nullable: false),
                    Mp = table.Column<int>(nullable: false),
                    MaxMp = table.Column<int>(nullable: false),
                    MaxHp = table.Column<int>(nullable: false),
                    Defense = table.Column<int>(nullable: false),
                    Speed = table.Column<float>(nullable: false),
                    Str = table.Column<int>(nullable: false),
                    Dex = table.Column<int>(nullable: false),
                    Int = table.Column<int>(nullable: false),
                    Luk = table.Column<int>(nullable: false),
                    Exp = table.Column<int>(nullable: false),
                    StatPoint = table.Column<int>(nullable: false),
                    posX = table.Column<float>(nullable: false),
                    posY = table.Column<float>(nullable: false),
                    posZ = table.Column<float>(nullable: false),
                    rotateY = table.Column<float>(nullable: false),
                    Money = table.Column<int>(nullable: false),
                    SkillPoint = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.PlayerDbId);
                    table.ForeignKey(
                        name: "FK_Player_Account_AccountDbId",
                        column: x => x.AccountDbId,
                        principalTable: "Account",
                        principalColumn: "AccountDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    Count = table.Column<int>(nullable: false),
                    Slot = table.Column<int>(nullable: false),
                    Equipped = table.Column<bool>(nullable: false),
                    OwnerDbId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemDbId);
                    table.ForeignKey(
                        name: "FK_Item_Player_OwnerDbId",
                        column: x => x.OwnerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuickSlot",
                columns: table => new
                {
                    QuickSlotDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    Slot = table.Column<string>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Skill",
                columns: table => new
                {
                    SkillDbId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(nullable: false),
                    SkillLevel = table.Column<int>(nullable: false),
                    PlayerDbId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skill", x => x.SkillDbId);
                    table.UniqueConstraint("AK_Skill_PlayerDbId_TemplateId", x => new { x.PlayerDbId, x.TemplateId });
                    table.ForeignKey(
                        name: "FK_Skill_Player_PlayerDbId",
                        column: x => x.PlayerDbId,
                        principalTable: "Player",
                        principalColumn: "PlayerDbId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_AccountLoginId",
                table: "Account",
                column: "AccountLoginId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Item_OwnerDbId",
                table: "Item",
                column: "OwnerDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_AccountDbId",
                table: "Player",
                column: "AccountDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Player_PlayerName",
                table: "Player",
                column: "PlayerName",
                unique: true,
                filter: "[PlayerName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "QuickSlot");

            migrationBuilder.DropTable(
                name: "Skill");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
