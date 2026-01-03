using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Billbyte_BE.Migrations
{
    /// <inheritdoc />
    public partial class UserAssignmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTableAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RestaurantId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    TablePreferenceId = table.Column<int>(type: "integer", nullable: false),
                    UserId1 = table.Column<int>(type: "integer", nullable: true),
                    TablePreferenceId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTableAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTableAssignments_TablePreferences_TablePreferenceId",
                        column: x => x.TablePreferenceId,
                        principalTable: "TablePreferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTableAssignments_TablePreferences_TablePreferenceId1",
                        column: x => x.TablePreferenceId1,
                        principalTable: "TablePreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserTableAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTableAssignments_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTableAssignments_RestaurantId_UserId_TablePreferenceId",
                table: "UserTableAssignments",
                columns: new[] { "RestaurantId", "UserId", "TablePreferenceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTableAssignments_TablePreferenceId",
                table: "UserTableAssignments",
                column: "TablePreferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTableAssignments_TablePreferenceId1",
                table: "UserTableAssignments",
                column: "TablePreferenceId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserTableAssignments_UserId",
                table: "UserTableAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTableAssignments_UserId1",
                table: "UserTableAssignments",
                column: "UserId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTableAssignments");
        }
    }
}
