using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminCmsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    subtitle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    imageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    link = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    startDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    endDate = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    clicks = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "CartRules",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    condition = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    action = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    priority = table.Column<int>(type: "int", nullable: false),
                    startsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    endsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartRules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    productIds = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    originalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Enquiries",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquiries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    isRead = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    discountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    discountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    minOrderValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    startsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    endsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "PricingRequests",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    customerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    product = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    requestedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRequests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Quotes",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    customerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    product = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    productName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    customerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    rating = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "StoreOffers",
                columns: table => new
                {
                    id = table.Column<string>(type: "char(36)", nullable: false),
                    storeId = table.Column<string>(type: "char(36)", nullable: false),
                    storeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    discountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    startsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    endsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreOffers", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_key",
                table: "AppSettings",
                column: "key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "CartRules");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "Enquiries");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "PricingRequests");

            migrationBuilder.DropTable(
                name: "Quotes");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "StoreOffers");
        }
    }
}
