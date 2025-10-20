using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCascadePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MatchesId",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MatchesId1",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_TeamStatistics_MatchesId1",
                table: "TeamStatistics",
                column: "MatchesId1");

            migrationBuilder.CreateIndex(
                name: "IX_PostPoneMatch_IdMatch",
                table: "PostPoneMatch",
                column: "IdMatch");

            migrationBuilder.CreateIndex(
                name: "IX_PostPoneMatch_IdTeamPostPone",
                table: "PostPoneMatch",
                column: "IdTeamPostPone");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamStatistics_Match_MatchesId1",
                table: "TeamStatistics",
                column: "MatchesId1",
                principalTable: "Match",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamStatistics_Match_MatchesId1",
                table: "TeamStatistics");

            migrationBuilder.DropTable(
                name: "PostPoneMatch");

            migrationBuilder.DropIndex(
                name: "IX_TeamStatistics_MatchesId1",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "MatchesId1",
                table: "TeamStatistics");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatchesId",
                table: "TeamStatistics",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
