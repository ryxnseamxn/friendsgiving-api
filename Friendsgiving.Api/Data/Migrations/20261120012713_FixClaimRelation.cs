using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Friendsgiving.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixClaimRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gatherings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HostName = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gatherings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DishItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GatheringId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishItems_Gatherings_GatheringId",
                        column: x => x.GatheringId,
                        principalTable: "Gatherings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DishItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestName = table.Column<string>(type: "text", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_DishItems_DishItemId",
                        column: x => x.DishItemId,
                        principalTable: "DishItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_DishItemId",
                table: "Claims",
                column: "DishItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DishItems_GatheringId",
                table: "DishItems",
                column: "GatheringId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "DishItems");

            migrationBuilder.DropTable(
                name: "Gatherings");
        }
    }
}
