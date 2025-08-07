using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniE_TicaretPaneli.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderItemProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Price",
                table: "OrderItems",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "SelectedSize",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CardNumberLastFour",
                table: "CreditCards",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "OrderItems");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumberLastFour",
                table: "CreditCards",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
