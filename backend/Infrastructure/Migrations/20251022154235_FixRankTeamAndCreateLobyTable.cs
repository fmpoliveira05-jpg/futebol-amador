using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRankTeamAndCreateLobyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdRank",
                table: "Team",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LobbyPresence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyPresence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbyPresence_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyPresence_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LobbyPresence_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LobbyPresence_MatchId",
                table: "LobbyPresence",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyPresence_TeamId",
                table: "LobbyPresence",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyPresence_UserId",
                table: "LobbyPresence",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team",
                column: "IdRank",
                principalTable: "Rank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team");

            migrationBuilder.DropTable(
                name: "LobbyPresence");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdRank",
                table: "Team",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team",
                column: "IdRank",
                principalTable: "Rank",
                principalColumn: "Id");
        }
    }
}
