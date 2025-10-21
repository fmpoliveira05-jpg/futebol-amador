using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPostPoneMatchError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Match_Chat_IdChat",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Match_Pitch_idPitch",
                table: "Match");

            migrationBuilder.DropForeignKey(
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Chat_IdChat",
                table: "Match",
                column: "IdChat",
                principalTable: "Chat",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Match_Pitch_idPitch",
                table: "Match",
                column: "idPitch",
                principalTable: "Pitch",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team",
                column: "IdCalendar",
                principalTable: "Calendar",
                principalColumn: "Id");
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
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team");

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
                name: "FK_Team_Calendar_IdCalendar",
                table: "Team",
                column: "IdCalendar",
                principalTable: "Calendar",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
