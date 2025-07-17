using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniE_TicaretPaneli.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_GenderCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_GenderCategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "GenderCategoryId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "FullPath",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullPath",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "GenderCategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Products_GenderCategoryId",
                table: "Products",
                column: "GenderCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_GenderCategoryId",
                table: "Products",
                column: "GenderCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
