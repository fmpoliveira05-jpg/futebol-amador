using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConcorrenciaEIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Antes das restrições únicas, retiram-se os duplicados que pedidos simultâneos possam ter
            // criado (fica o mais antigo). Nomes de equipas repetidos não se corrigem sozinhos: a
            // migração falha e é preciso renomear à mão (ver docs/OPERACAO.md).
            migrationBuilder.Sql(@"
WITH Repetidos AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY IdPlayer, IdTeam ORDER BY InviteDate, Id) AS N
    FROM MembershipRequests)
DELETE FROM Repetidos WHERE N > 1;");

            migrationBuilder.Sql(@"
WITH Repetidos AS (
    SELECT Id, ROW_NUMBER() OVER (PARTITION BY IdSender, IdReceiver, GameDate ORDER BY Id) AS N
    FROM MatchInvite)
DELETE FROM Repetidos WHERE N > 1;");

            migrationBuilder.DropIndex(
                name: "IX_TransferOffer_PlayerId",
                table: "TransferOffer");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_MatchesId",
                table: "TeamStatistics");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_IdPlayer",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_IdSender",
                table: "MatchInvite");

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "TransferOffer",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "TeamStatistics",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "Team",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "Season",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "PostPoneMatch",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AlteradoEm",
                table: "Player",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "Player",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "MatchInvite",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Versao",
                table: "Match",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffer_PlayerId_IdToTeam",
                table: "TransferOffer",
                columns: new[] { "PlayerId", "IdToTeam" },
                unique: true,
                filter: "[Status] IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId_IdTeam",
                table: "TeamStatistics",
                columns: new[] { "MatchesId", "IdTeam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Team_CurrentPoints",
                table: "Team",
                column: "CurrentPoints");

            migrationBuilder.CreateIndex(
                name: "IX_Team_Name",
                table: "Team",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Season_Status_StartDate",
                table: "Season",
                columns: new[] { "Status", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdPlayer_IdTeam",
                table: "MembershipRequests",
                columns: new[] { "IdPlayer", "IdTeam" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdSender_IdReceiver_GameDate",
                table: "MatchInvite",
                columns: new[] { "IdSender", "IdReceiver", "GameDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Match_MatchDate",
                table: "Match",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_Match_MatchStatus_MatchDate",
                table: "Match",
                columns: new[] { "MatchStatus", "MatchDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_TransferOffer_PlayerId_IdToTeam",
                table: "TransferOffer");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_MatchesId_IdTeam",
                table: "TeamStatistics");

            migrationBuilder.DropIndex(
                name: "IX_Team_CurrentPoints",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Team_Name",
                table: "Team");

            migrationBuilder.DropIndex(
                name: "IX_Season_Status_StartDate",
                table: "Season");

            migrationBuilder.DropIndex(
                name: "IX_MembershipRequests_IdPlayer_IdTeam",
                table: "MembershipRequests");

            migrationBuilder.DropIndex(
                name: "IX_MatchInvite_IdSender_IdReceiver_GameDate",
                table: "MatchInvite");

            migrationBuilder.DropIndex(
                name: "IX_Match_MatchDate",
                table: "Match");

            migrationBuilder.DropIndex(
                name: "IX_Match_MatchStatus_MatchDate",
                table: "Match");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "TransferOffer");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Season");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "PostPoneMatch");

            migrationBuilder.DropColumn(
                name: "AlteradoEm",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Player");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "MatchInvite");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "Match");

            migrationBuilder.CreateIndex(
                name: "IX_TransferOffer_PlayerId",
                table: "TransferOffer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId",
                table: "TeamStatistics",
                column: "MatchesId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipRequests_IdPlayer",
                table: "MembershipRequests",
                column: "IdPlayer");

            migrationBuilder.CreateIndex(
                name: "IX_MatchInvite_IdSender",
                table: "MatchInvite",
                column: "IdSender");
        }
    }
}
