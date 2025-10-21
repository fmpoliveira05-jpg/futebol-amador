using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCancelledMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CancelledMatch_Match_IdMatch",
                table: "CancelledMatch");

            migrationBuilder.DropForeignKey(
                name: "FK_CancelledMatch_Team_IdTeam",
                table: "CancelledMatch");

            migrationBuilder.AddForeignKey(
                name: "FK_CancelledMatch_Match_IdMatch",
                table: "CancelledMatch",
                column: "IdMatch",
                principalTable: "Match",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CancelledMatch_Team_IdTeam",
                table: "CancelledMatch",
                column: "IdTeam",
                principalTable: "Team",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CancelledMatch_Match_IdMatch",
                table: "CancelledMatch");

            migrationBuilder.DropForeignKey(
                name: "FK_CancelledMatch_Team_IdTeam",
                table: "CancelledMatch");

            migrationBuilder.AddForeignKey(
                name: "FK_CancelledMatch_Match_IdMatch",
                table: "CancelledMatch",
                column: "IdMatch",
                principalTable: "Match",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CancelledMatch_Team_IdTeam",
                table: "CancelledMatch",
                column: "IdTeam",
                principalTable: "Team",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
