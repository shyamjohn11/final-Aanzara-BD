using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerShopManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dealerId",
                table: "Products",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    dealerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    shopName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ownerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    alternatePhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    state = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    pincode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gstNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    panNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    shopDescription = table.Column<string>(type: "text", nullable: true),
                    shopLogo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Dealers_Agents_agentId",
                        column: x => x.agentId,
                        principalTable: "Agents",
                        principalColumn: "agentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_DealerId",
                table: "Products",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_AgentId",
                table: "Dealers",
                column: "agentId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_Status",
                table: "Dealers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UX_Dealers_DealerCode",
                table: "Dealers",
                column: "dealerCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Dealers_dealerId",
                table: "Products",
                column: "dealerId",
                principalTable: "Dealers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Dealers_dealerId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_Products_DealerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "dealerId",
                table: "Products");
        }
    }
}
