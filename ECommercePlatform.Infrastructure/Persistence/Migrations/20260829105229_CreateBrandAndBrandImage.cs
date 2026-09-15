using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateBrandAndBrandImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    brandId = table.Column<string>(type: "char(36)", nullable: false),
                    brandName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    isOnSale = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.brandId);
                });

            migrationBuilder.CreateTable(
                name: "BrandImages",
                columns: table => new
                {
                    brandImageId = table.Column<string>(type: "char(36)", nullable: false),
                    brandId = table.Column<string>(type: "char(36)", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    isPrimary = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandImages", x => x.brandImageId);
                    table.ForeignKey(
                        name: "FK_BrandImages_Brands_brandId",
                        column: x => x.brandId,
                        principalTable: "Brands",
                        principalColumn: "brandId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandImages_brandId",
                table: "BrandImages",
                column: "brandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "brandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "BrandImages");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Products_BrandId",
                table: "Products");
        }
    }
}
