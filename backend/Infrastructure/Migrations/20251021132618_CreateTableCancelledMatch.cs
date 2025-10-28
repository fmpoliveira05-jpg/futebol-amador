using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateTableCancelledMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancelledMatch",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTeam = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdMatch = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TimeCancellation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelledMatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancelledMatch_Match_IdMatch",
                        column: x => x.IdMatch,
                        principalTable: "Match",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CancelledMatch_Team_IdTeam",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancelledMatch");
        }
    }
}
