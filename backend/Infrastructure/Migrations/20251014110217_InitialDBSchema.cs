using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialDBSchema : Migration
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    countMessages = table.Column<int>(type: "int", nullable: false)
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
                    winPoints = table.Column<int>(type: "int", nullable: false),
                    drawPoints = table.Column<int>(type: "int", nullable: false),
                    losePoints = table.Column<int>(type: "int", nullable: false),
                    PointsToPromotion = table.Column<int>(type: "int", nullable: false),
                    idNextRank = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    idPreviousRank = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rank", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rank_Rank_idNextRank",
                        column: x => x.idNextRank,
                        principalTable: "Rank",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Match",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchStatus = table.Column<int>(type: "int", nullable: false),
                    TeamsCount = table.Column<int>(type: "int", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    iscompetive = table.Column<bool>(type: "bit", nullable: false),
                    PitchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    idPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        name: "FK_Match_Pitch_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Icon = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PitchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataFoundation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MemberCount = table.Column<int>(type: "int", nullable: false),
                    AdminCount = table.Column<int>(type: "int", nullable: false),
                    AverageAge = table.Column<float>(type: "real", nullable: false),
                    CurrentPoints = table.Column<int>(type: "int", nullable: false),
                    RankId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IdRank = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountMemvberShipsRequests = table.Column<int>(type: "int", nullable: false),
                    CountMatchesInvites = table.Column<int>(type: "int", nullable: false),
                    CalendarId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdCalendar = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Team_Calendar_CalendarId",
                        column: x => x.CalendarId,
                        principalTable: "Calendar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Team_Pitch_PitchId",
                        column: x => x.PitchId,
                        principalTable: "Pitch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Team_Rank_RankId",
                        column: x => x.RankId,
                        principalTable: "Rank",
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
                    PitchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdPitch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdChat = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchInvite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchInvite_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchInvite_Pitch_PitchId",
                        column: x => x.PitchId,
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
                name: "TeamStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchResult = table.Column<int>(type: "int", nullable: false),
                    MatchesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                        name: "FK_TeamStatistics_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    address = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PhoneNumber = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Position = table.Column<int>(type: "int", nullable: true),
                    height = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    idTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MembershipRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    idPlayer = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    idTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    inviteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    sender = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipRequests_Team_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MembershipRequests_User_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    actorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    idUser = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    timeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                        name: "FK_Message_User_actorId",
                        column: x => x.actorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Match_CalendarId",
                table: "Match",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Match_PitchId",
                table: "Match",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_ChatId",
                table: "MatchInvite",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdReceiver",
                table: "MatchInvite",
                column: "IdReceiver");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdSender",
                table: "MatchInvite",
                column: "IdSender");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_PitchId",
                table: "MatchInvite",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_PlayerId",
                table: "MembershipRequests",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_TeamId",
                table: "MembershipRequests",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_actorId",
                table: "Message",
                column: "actorId");

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatId",
                table: "Message",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_Rank_idNextRank",
                table: "Rank",
                column: "idNextRank",
                unique: true,
                filter: "[idNextRank] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Team_CalendarId",
                table: "Team",
                column: "CalendarId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_PitchId",
                table: "Team",
                column: "PitchId");

            migrationBuilder.CreateIndex(
                name: "IX_Team_RankId",
                table: "Team",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId",
                table: "TeamStatistics",
                column: "MatchesId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_TeamId",
                table: "TeamStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_User_TeamId",
                table: "User",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MatchInvite");

            migrationBuilder.DropTable(
                name: "MembershipRequests");

            migrationBuilder.DropTable(
                name: "Message");

            migrationBuilder.DropTable(
                name: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "Chat");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Match");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropTable(
                name: "Calendar");

            migrationBuilder.DropTable(
                name: "Pitch");

            migrationBuilder.DropTable(
                name: "Rank");
        }
    }
}
