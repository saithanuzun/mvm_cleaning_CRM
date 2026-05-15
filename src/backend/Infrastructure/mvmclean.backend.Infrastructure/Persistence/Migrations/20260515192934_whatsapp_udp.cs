using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mvmclean.backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class whatsapp_udp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber_Value",
                table: "WhatsAppChats",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<bool>(
                name: "IsGroup",
                table: "WhatsAppChats",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGroup",
                table: "WhatsAppChats");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber_Value",
                table: "WhatsAppChats",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
