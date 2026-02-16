using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvmclean.backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingItemDiscountFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "BookingItem",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice_Amount",
                table: "BookingItem",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalPrice_Currency",
                table: "BookingItem",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "BookingItem");

            migrationBuilder.DropColumn(
                name: "OriginalPrice_Amount",
                table: "BookingItem");

            migrationBuilder.DropColumn(
                name: "OriginalPrice_Currency",
                table: "BookingItem");
        }
    }
}
