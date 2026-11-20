using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendsgiving.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAllergies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GatheringId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestName = table.Column<string>(type: "text", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Allergies_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allergies_GatheringId_GuestName",
                table: "Allergies",
                columns: new[] { "GatheringId", "GuestName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allergies");
        }
    }
}
