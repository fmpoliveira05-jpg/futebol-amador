using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataBaseFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rank_Rank_idNextRank",
                table: "Rank");

            migrationBuilder.DropIndex(
                name: "IX_Rank_idNextRank",
                table: "Rank");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "User",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "CountMatchesInvites",
                table: "Team",
                newName: "CountSendInvites");

            migrationBuilder.RenameColumn(
                name: "winPoints",
                table: "Rank",
                newName: "WinPoints");

            migrationBuilder.RenameColumn(
                name: "losePoints",
                table: "Rank",
                newName: "LosePoints");

            migrationBuilder.RenameColumn(
                name: "idPreviousRank",
                table: "Rank",
                newName: "IdPreviousRank");

            migrationBuilder.RenameColumn(
                name: "idNextRank",
                table: "Rank",
                newName: "IdNextRank");

            migrationBuilder.RenameColumn(
                name: "drawPoints",
                table: "Rank",
                newName: "DrawPoints");

            migrationBuilder.RenameColumn(
                name: "iscompetive",
                table: "Match",
                newName: "Iscompetive");

            migrationBuilder.AddColumn<int>(
                name: "counterMembershipRequests",
                table: "User",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Num_goals",
                table: "TeamStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "IdRank",
                table: "Team",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "CountReceivedIntes",
                table: "Team",
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
                name: "IX_Rank_IdNextRank",
                table: "Rank",
                column: "IdNextRank",
                unique: true,
                filter: "[IdNextRank] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Rank_Rank_IdNextRank",
                table: "Rank",
                column: "IdNextRank",
                principalTable: "Rank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rank_Rank_IdNextRank",
                table: "Rank");

            migrationBuilder.DropIndex(
                name: "IX_Rank_IdNextRank",
                table: "Rank");

            migrationBuilder.DropColumn(
                name: "counterMembershipRequests",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Num_goals",
                table: "TeamStatistics");

            migrationBuilder.DropColumn(
                name: "CountReceivedIntes",
                table: "Team");

            migrationBuilder.DropColumn(
                name: "countMatches",
                table: "Calendar");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "User",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "CountSendInvites",
                table: "Team",
                newName: "CountMatchesInvites");

            migrationBuilder.RenameColumn(
                name: "WinPoints",
                table: "Rank",
                newName: "winPoints");

            migrationBuilder.RenameColumn(
                name: "LosePoints",
                table: "Rank",
                newName: "losePoints");

            migrationBuilder.RenameColumn(
                name: "IdPreviousRank",
                table: "Rank",
                newName: "idPreviousRank");

            migrationBuilder.RenameColumn(
                name: "IdNextRank",
                table: "Rank",
                newName: "idNextRank");

            migrationBuilder.RenameColumn(
                name: "DrawPoints",
                table: "Rank",
                newName: "drawPoints");

            migrationBuilder.RenameColumn(
                name: "Iscompetive",
                table: "Match",
                newName: "iscompetive");

            migrationBuilder.AlterColumn<Guid>(
                name: "IdRank",
                table: "Team",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rank_idNextRank",
                table: "Rank",
                column: "idNextRank",
                unique: true,
                filter: "[idNextRank] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Rank_Rank_idNextRank",
                table: "Rank",
                column: "idNextRank",
                principalTable: "Rank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
