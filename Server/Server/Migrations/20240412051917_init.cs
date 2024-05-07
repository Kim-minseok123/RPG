using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class init : Migration
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
                    AccountDbId = table.Column<int>(nullable: false),
                    PlayerClass = table.Column<int>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Hp = table.Column<int>(nullable: false),
                    Mp = table.Column<int>(nullable: false),
                    MaxHp = table.Column<int>(nullable: false),
                    Attack = table.Column<int>(nullable: false),
                    Speed = table.Column<float>(nullable: false),
                    TotalExp = table.Column<int>(nullable: false)
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
                name: "Player");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
