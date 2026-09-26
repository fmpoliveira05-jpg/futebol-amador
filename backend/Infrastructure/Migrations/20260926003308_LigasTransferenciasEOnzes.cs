using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LigasTransferenciasEOnzes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Fouls",
                table: "TeamStatistics",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Team",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdLeague",
                table: "Team",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "PostPoneMatch",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfBirth",
                table: "Player",
                type: "nvarchar(56)",
                maxLength: 56,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedTeamAt",
                table: "Player",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nationality",
                table: "Player",
                type: "nvarchar(56)",
                maxLength: 56,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredFoot",
                table: "Player",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Player",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "Player",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdHomeTeam",
                table: "Match",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdSeason",
                table: "Match",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostponeReason",
                table: "Match",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostponedFrom",
                table: "Match",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Round",
                table: "Match",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewDate",
                table: "CancelledMatch",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginalDate",
                table: "CancelledMatch",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "League",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    PromotionSpots = table.Column<int>(type: "int", nullable: false),
                    RelegationSpots = table.Column<int>(type: "int", nullable: false),
                    SeasonDurationDays = table.Column<int>(type: "int", nullable: false),
                    TrophyName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_League", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MatchEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMatch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Minute = table.Column<int>(type: "int", nullable: true),
                    PlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    RelatedPlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchEvent_Match_IdMatch",
                        column: x => x.IdMatch,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchLineup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMatch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Formation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsAutoFilled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchLineup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchLineup_Match_IdMatch",
                        column: x => x.IdMatch,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchLineup_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamTitle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSeason = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLeague = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrophyName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LeagueName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SeasonName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WonAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamTitle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamTitle_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferListing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ListedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferListing", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferListing_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferListing_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferOffer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IdFromTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdToTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ViaListing = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferOffer_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferOffer_Team_IdFromTeam",
                        column: x => x.IdFromTeam,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TransferOffer_Team_IdToTeam",
                        column: x => x.IdToTeam,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TransferRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IdFromTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FromTeamName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IdToTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToTeamName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Season",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLeague = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Season", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Season_League_IdLeague",
                        column: x => x.IdLeague,
                        principalTable: "League",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LineupSlot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdLineup = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsStarter = table.Column<bool>(type: "bit", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false),
                    PositionCode = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineupSlot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineupSlot_MatchLineup_IdLineup",
                        column: x => x.IdLineup,
                        principalTable: "MatchLineup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineupSlot_Player_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Player",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SeasonTeam",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSeason = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonTeam", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonTeam_Season_IdSeason",
                        column: x => x.IdSeason,
                        principalTable: "Season",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonTeam_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Team_IdLeague",
                table: "Team",
                column: "IdLeague");

            migrationBuilder.CreateIndex(
                name: "IX_Match_IdSeason",
                table: "Match",
                column: "IdSeason");

            migrationBuilder.CreateIndex(
                name: "IX_League_Level",
                table: "League",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LineupSlot_IdLineup",
                table: "LineupSlot",
                column: "IdLineup");

            migrationBuilder.CreateIndex(
                name: "IX_LineupSlot_PlayerId",
                table: "LineupSlot",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvent_IdMatch",
                table: "MatchEvent",
                column: "IdMatch");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvent_PlayerId",
                table: "MatchEvent",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineup_IdMatch_IdTeam",
                table: "MatchLineup",
                columns: new[] { "IdMatch", "IdTeam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineup_IdTeam",
                table: "MatchLineup",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_Season_IdLeague",
                table: "Season",
                column: "IdLeague");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeam_IdSeason_IdTeam",
                table: "SeasonTeam",
                columns: new[] { "IdSeason", "IdTeam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeasonTeam_IdTeam",
                table: "SeasonTeam",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TeamTitle_IdTeam",
                table: "TeamTitle",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TransferListing_IdTeam",
                table: "TransferListing",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TransferListing_PlayerId",
                table: "TransferListing",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffer_IdFromTeam",
                table: "TransferOffer",
                column: "IdFromTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffer_IdToTeam",
                table: "TransferOffer",
                column: "IdToTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffer_PlayerId",
                table: "TransferOffer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferRecord_PlayerId",
                table: "TransferRecord",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Season_IdSeason",
                table: "Match",
                column: "IdSeason",
                principalTable: "Season",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Team_League_IdLeague",
                table: "Team",
                column: "IdLeague",
                principalTable: "League",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Season_IdSeason",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_League_IdLeague",
                table: "Team");

            migrationBuilder.DropTable(
                name: "LineupSlot");

            migrationBuilder.DropTable(
                name: "MatchEvent");

            migrationBuilder.DropTable(
                name: "SeasonTeam");

            migrationBuilder.DropTable(
                name: "TeamTitle");

            migrationBuilder.DropTable(
                name: "TransferListing");

            migrationBuilder.DropTable(
                name: "TransferOffer");

            migrationBuilder.DropTable(
                name: "TransferRecord");

            migrationBuilder.DropTable(
                name: "MatchLineup");

            migrationBuilder.DropTable(
                name: "Season");

            migrationBuilder.DropTable(
                name: "League");

            migrationBuilder.DropIndex(
                name: "IX_Team_IdLeague",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Match_IdSeason",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Fouls",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "IdLeague",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "PostPoneMatch");

            migrationBuilder.DropColumn(
                name: "CountryOfBirth",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "JoinedTeamAt",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Nationality",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "PreferredFoot",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "IdHomeTeam",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "IdSeason",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "PostponeReason",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "PostponedFrom",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Round",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "NewDate",
                table: "CancelledMatch");

            migrationBuilder.DropColumn(
                name: "OriginalDate",
                table: "CancelledMatch");
        }
    }
}
