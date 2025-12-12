using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Billbyte_BE.Migrations
{
    /// <inheritdoc />
    public partial class updateMneuItemTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "CGSTPercentage",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "FoodTypeId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "GSTPercentage",
                table: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "ItemName",
                table: "MenuItems",
                newName: "VegType");

            migrationBuilder.RenameColumn(
                name: "ItemCost",
                table: "MenuItems",
                newName: "Price");

            migrationBuilder.AddColumn<string>(
                name: "MenuId",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "MenuItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems",
                column: "MenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "MenuId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "VegType",
                table: "MenuItems",
                newName: "ItemName");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "MenuItems",
                newName: "ItemCost");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "MenuItems",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<decimal>(
                name: "CGSTPercentage",
                table: "MenuItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoodTypeId",
                table: "MenuItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GSTPercentage",
                table: "MenuItems",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems",
                column: "Id");
        }
    }
}
