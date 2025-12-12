using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billbyte_BE.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuItemImgTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Base64Image",
                table: "MenuItemImgs",
                newName: "ItemImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ItemImage",
                table: "MenuItemImgs",
                newName: "Base64Image");
        }
    }
}
