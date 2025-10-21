using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRelationshipTeamStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamStatistics_Match_MatchesId1",
                table: "TeamStatistics");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_MatchesId1",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "MatchesId1",
                table: "TeamStatistics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MatchesId1",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId1",
                table: "TeamStatistics",
                column: "MatchesId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStatistics_Match_MatchesId1",
                table: "TeamStatistics",
                column: "MatchesId1",
                principalTable: "Match",
                principalColumn: "Id");
        }
    }
}
