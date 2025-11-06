using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigracaoTeste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Calendar",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Calendar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pitch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pitch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rank",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WinPoints = table.Column<int>(type: "int", nullable: false),
                    DrawPoints = table.Column<int>(type: "int", nullable: false),
                    LosePoints = table.Column<int>(type: "int", nullable: false),
                    PointsToPromotion = table.Column<int>(type: "int", nullable: false),
                    IdNextRank = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdPreviousRank = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rank_Rank_IdNextRank",
                        column: x => x.IdNextRank,
                        principalTable: "Rank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rank_Rank_IdPreviousRank",
                        column: x => x.IdPreviousRank,
                        principalTable: "Rank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchStatus = table.Column<int>(type: "int", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompetive = table.Column<bool>(type: "bit", nullable: false),
                    idPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdChat = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Match", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Match_Calendar_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Match_Chat_IdChat",
                        column: x => x.IdChat,
                        principalTable: "Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Match_Pitch_idPitch",
                        column: x => x.idPitch,
                        principalTable: "Pitch",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Icon = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    IdPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataFoundation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentPoints = table.Column<int>(type: "int", nullable: false),
                    IdRank = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCalendar = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_Calendar_IdCalendar",
                        column: x => x.IdCalendar,
                        principalTable: "Calendar",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Team_Pitch_IdPitch",
                        column: x => x.IdPitch,
                        principalTable: "Pitch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Team_Rank_IdRank",
                        column: x => x.IdRank,
                        principalTable: "Rank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Message_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Message_User_IdUser",
                        column: x => x.IdUser,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdmin",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdmin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuperAdmin_User_Id",
                        column: x => x.Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancelledMatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMatch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TimeCancellation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelledMatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancelledMatch_Match_IdMatch",
                        column: x => x.IdMatch,
                        principalTable: "Match",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CancelledMatch_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MatchInvite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdSender = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdReceiver = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdChat = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchInvite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchInvite_Chat_IdChat",
                        column: x => x.IdChat,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchInvite_Pitch_IdPitch",
                        column: x => x.IdPitch,
                        principalTable: "Pitch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchInvite_Team_IdReceiver",
                        column: x => x.IdReceiver,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatchInvite_Team_IdSender",
                        column: x => x.IdSender,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IsAdminLastChangedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Player_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Player_User_Id",
                        column: x => x.Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostPoneMatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeamPostPone = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMatch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostPoneDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPoneMatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPoneMatch_Match_IdMatch",
                        column: x => x.IdMatch,
                        principalTable: "Match",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PostPoneMatch_Team_IdTeamPostPone",
                        column: x => x.IdTeamPostPone,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumGoals = table.Column<int>(type: "int", nullable: false),
                    MatchesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchResult = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Match_MatchesId",
                        column: x => x.MatchesId,
                        principalTable: "Match",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TeamStatistics_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MembershipRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPlayer = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InviteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPlayerSender = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipRequests_Player_IdPlayer",
                        column: x => x.IdPlayer,
                        principalTable: "Player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipRequests_Team_IdTeam",
                        column: x => x.IdTeam,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancelledMatch_IdMatch",
                table: "CancelledMatch",
                column: "IdMatch");

            migrationBuilder.CreateIndex(
                name: "IX_CancelledMatch_IdTeam",
                table: "CancelledMatch",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_Match_CalendarId",
                table: "Match",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_IdChat",
                table: "Match",
                column: "IdChat");

            migrationBuilder.CreateIndex(
                name: "IX_Match_idPitch",
                table: "Match",
                column: "idPitch");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdChat",
                table: "MatchInvite",
                column: "IdChat");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdPitch",
                table: "MatchInvite",
                column: "IdPitch");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdReceiver",
                table: "MatchInvite",
                column: "IdReceiver");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdSender",
                table: "MatchInvite",
                column: "IdSender");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdPlayer",
                table: "MembershipRequests",
                column: "IdPlayer");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdTeam",
                table: "MembershipRequests",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatId",
                table: "Message",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_IdUser",
                table: "Message",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_Player_IdTeam",
                table: "Player",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_PostPoneMatch_IdMatch",
                table: "PostPoneMatch",
                column: "IdMatch");

            migrationBuilder.CreateIndex(
                name: "IX_PostPoneMatch_IdTeamPostPone",
                table: "PostPoneMatch",
                column: "IdTeamPostPone");

            migrationBuilder.CreateIndex(
                name: "IX_Rank_IdNextRank",
                table: "Rank",
                column: "IdNextRank",
                unique: true,
                filter: "[IdNextRank] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Rank_IdPreviousRank",
                table: "Rank",
                column: "IdPreviousRank",
                unique: true,
                filter: "[IdPreviousRank] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Team_IdCalendar",
                table: "Team",
                column: "IdCalendar");

            migrationBuilder.CreateIndex(
                name: "IX_Team_IdPitch",
                table: "Team",
                column: "IdPitch");

            migrationBuilder.CreateIndex(
                name: "IX_Team_IdRank",
                table: "Team",
                column: "IdRank");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_IdTeam",
                table: "TeamStatistics",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId",
                table: "TeamStatistics",
                column: "MatchesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancelledMatch");

            migrationBuilder.DropTable(
                name: "MatchInvite");

            migrationBuilder.DropTable(
                name: "MembershipRequests");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "PostPoneMatch");

            migrationBuilder.DropTable(
                name: "SuperAdmin");

            migrationBuilder.DropTable(
                name: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "Calendar");

            migrationBuilder.DropTable(
                name: "Pitch");

            migrationBuilder.DropTable(
                name: "Rank");
        }
    }
}
