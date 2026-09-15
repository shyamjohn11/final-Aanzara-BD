using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTypeAndLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "Notifications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "system");

            migrationBuilder.AddColumn<string>(
                name: "link",
                table: "Notifications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "link",
                table: "Notifications");
        }
    }
}
