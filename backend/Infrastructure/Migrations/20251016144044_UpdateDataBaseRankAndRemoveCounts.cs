using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseRankAndRemoveCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Pitch_PitchId",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchInvite_Chat_ChatId",
                table: "MatchInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchInvite_Pitch_PitchId",
                table: "MatchInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipRequests_Team_TeamId",
                table: "MembershipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipRequests_User_PlayerId",
                table: "MembershipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_actorId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Calendar_CalendarId",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Pitch_PitchId",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Rank_RankId",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStatistics_Team_TeamId",
                table: "TeamStatistics");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Team_TeamId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_TeamId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_TeamId",
                table: "TeamStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Team_CalendarId",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_PitchId",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_RankId",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Message_actorId",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_PlayerId",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_TeamId",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_ChatId",
                table: "MatchInvite");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_PitchId",
                table: "MatchInvite");

            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Position",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "User");

            migrationBuilder.DropColumn(
                name: "counterMembershipRequests",
                table: "User");

            migrationBuilder.DropColumn(
                name: "height",
                table: "User");

            migrationBuilder.DropColumn(
                name: "idTeam",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "AdminCount",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "CalendarId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "CountMemvberShipsRequests",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "CountReceivedIntes",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "CountSendInvites",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "MemberCount",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "PitchId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "RankId",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "actorId",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "MembershipRequests");

            migrationBuilder.DropColumn(
                name: "TeamId",
                table: "MembershipRequests");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "MatchInvite");

            migrationBuilder.DropColumn(
                name: "PitchId",
                table: "MatchInvite");

            migrationBuilder.DropColumn(
                name: "TeamsCount",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "countMessages",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "countMatches",
                table: "Calendar");

            migrationBuilder.RenameColumn(
                name: "Num_goals",
                table: "TeamStatistics",
                newName: "NumGoals");

            migrationBuilder.RenameColumn(
                name: "idUser",
                table: "Message",
                newName: "IdUser");

            migrationBuilder.RenameColumn(
                name: "sender",
                table: "MembershipRequests",
                newName: "Sender");

            migrationBuilder.RenameColumn(
                name: "inviteDate",
                table: "MembershipRequests",
                newName: "InviteDate");

            migrationBuilder.RenameColumn(
                name: "idTeam",
                table: "MembershipRequests",
                newName: "IdTeam");

            migrationBuilder.RenameColumn(
                name: "idPlayer",
                table: "MembershipRequests",
                newName: "IdPlayer");

            migrationBuilder.RenameColumn(
                name: "Iscompetive",
                table: "Match",
                newName: "IsCompetive");

            migrationBuilder.RenameColumn(
                name: "PitchId",
                table: "Match",
                newName: "IdChat");

            migrationBuilder.RenameIndex(
                name: "IX_Match_PitchId",
                table: "Match",
                newName: "IX_Match_IdChat");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdTeam",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Player",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    idTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Player", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Player_Team_TeamId",
                        column: x => x.TeamId,
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
                name: "SuperAdmin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_IdTeam",
                table: "TeamStatistics",
                column: "IdTeam");

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
                name: "IX_Rank_IdPreviousRank",
                table: "Rank",
                column: "IdPreviousRank",
                unique: true,
                filter: "[IdPreviousRank] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Message_IdUser",
                table: "Message",
                column: "IdUser");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdPlayer",
                table: "MembershipRequests",
                column: "IdPlayer");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdTeam",
                table: "MembershipRequests",
                column: "IdTeam");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdChat",
                table: "MatchInvite",
                column: "IdChat");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdPitch",
                table: "MatchInvite",
                column: "IdPitch");

            migrationBuilder.CreateIndex(
                name: "IX_Match_idPitch",
                table: "Match",
                column: "idPitch");

            migrationBuilder.CreateIndex(
                name: "IX_Player_TeamId",
                table: "Player",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Chat_IdChat",
                table: "Match",
                column: "IdChat",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Pitch_idPitch",
                table: "Match",
                column: "idPitch",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchInvite_Chat_IdChat",
                table: "MatchInvite",
                column: "IdChat",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchInvite_Pitch_IdPitch",
                table: "MatchInvite",
                column: "IdPitch",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipRequests_Player_IdPlayer",
                table: "MembershipRequests",
                column: "IdPlayer",
                principalTable: "Player",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipRequests_Team_IdTeam",
                table: "MembershipRequests",
                column: "IdTeam",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_IdUser",
                table: "Message",
                column: "IdUser",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rank_Rank_IdPreviousRank",
                table: "Rank",
                column: "IdPreviousRank",
                principalTable: "Rank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team",
                column: "IdCalendar",
                principalTable: "Calendar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Pitch_IdPitch",
                table: "Team",
                column: "IdPitch",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team",
                column: "IdRank",
                principalTable: "Rank",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStatistics_Team_IdTeam",
                table: "TeamStatistics",
                column: "IdTeam",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Chat_IdChat",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Pitch_idPitch",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchInvite_Chat_IdChat",
                table: "MatchInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchInvite_Pitch_IdPitch",
                table: "MatchInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipRequests_Player_IdPlayer",
                table: "MembershipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_MembershipRequests_Team_IdTeam",
                table: "MembershipRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_IdUser",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Rank_Rank_IdPreviousRank",
                table: "Rank");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Pitch_IdPitch",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Rank_IdRank",
                table: "Team");

            migrationBuilder.DropForeignKey(
                name: "FK_TeamStatistics_Team_IdTeam",
                table: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "Player");

            migrationBuilder.DropTable(
                name: "SuperAdmin");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_IdTeam",
                table: "TeamStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Team_IdCalendar",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_IdPitch",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_IdRank",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Rank_IdPreviousRank",
                table: "Rank");

            migrationBuilder.DropIndex(
                name: "IX_Message_IdUser",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_IdPlayer",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_IdTeam",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_IdChat",
                table: "MatchInvite");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_IdPitch",
                table: "MatchInvite");

            migrationBuilder.DropIndex(
                name: "IX_Match_idPitch",
                table: "Match");

            migrationBuilder.RenameColumn(
                name: "NumGoals",
                table: "TeamStatistics",
                newName: "Num_goals");

            migrationBuilder.RenameColumn(
                name: "IdUser",
                table: "Message",
                newName: "idUser");

            migrationBuilder.RenameColumn(
                name: "Sender",
                table: "MembershipRequests",
                newName: "sender");

            migrationBuilder.RenameColumn(
                name: "InviteDate",
                table: "MembershipRequests",
                newName: "inviteDate");

            migrationBuilder.RenameColumn(
                name: "IdTeam",
                table: "MembershipRequests",
                newName: "idTeam");

            migrationBuilder.RenameColumn(
                name: "IdPlayer",
                table: "MembershipRequests",
                newName: "idPlayer");

            migrationBuilder.RenameColumn(
                name: "IsCompetive",
                table: "Match",
                newName: "Iscompetive");

            migrationBuilder.RenameColumn(
                name: "IdChat",
                table: "Match",
                newName: "PitchId");

            migrationBuilder.RenameIndex(
                name: "IX_Match_IdChat",
                table: "Match",
                newName: "IX_Match_PitchId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "User",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Position",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "User",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "counterMembershipRequests",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "idTeam",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IdTeam",
                table: "TeamStatistics",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "AdminCount",
                table: "Team",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CalendarId",
                table: "Team",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "CountMemvberShipsRequests",
                table: "Team",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountReceivedIntes",
                table: "Team",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CountSendInvites",
                table: "Team",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberCount",
                table: "Team",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PitchId",
                table: "Team",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "RankId",
                table: "Team",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "actorId",
                table: "Message",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PlayerId",
                table: "MembershipRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                table: "MembershipRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ChatId",
                table: "MatchInvite",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PitchId",
                table: "MatchInvite",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "TeamsCount",
                table: "Match",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "countMessages",
                table: "Chat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "countMatches",
                table: "Calendar",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_TeamId",
                table: "User",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_TeamId",
                table: "TeamStatistics",
                column: "TeamId");

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
                name: "IX_Message_actorId",
                table: "Message",
                column: "actorId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_PlayerId",
                table: "MembershipRequests",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_TeamId",
                table: "MembershipRequests",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_ChatId",
                table: "MatchInvite",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_PitchId",
                table: "MatchInvite",
                column: "PitchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Pitch_PitchId",
                table: "Match",
                column: "PitchId",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchInvite_Chat_ChatId",
                table: "MatchInvite",
                column: "ChatId",
                principalTable: "Chat",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchInvite_Pitch_PitchId",
                table: "MatchInvite",
                column: "PitchId",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipRequests_Team_TeamId",
                table: "MembershipRequests",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MembershipRequests_User_PlayerId",
                table: "MembershipRequests",
                column: "PlayerId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_actorId",
                table: "Message",
                column: "actorId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Calendar_CalendarId",
                table: "Team",
                column: "CalendarId",
                principalTable: "Calendar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Pitch_PitchId",
                table: "Team",
                column: "PitchId",
                principalTable: "Pitch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Rank_RankId",
                table: "Team",
                column: "RankId",
                principalTable: "Rank",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStatistics_Team_TeamId",
                table: "TeamStatistics",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Team_TeamId",
                table: "User",
                column: "TeamId",
                principalTable: "Team",
                principalColumn: "Id");
        }
    }
}
