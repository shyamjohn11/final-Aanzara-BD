using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreAndStoreImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    storeId = table.Column<string>(type: "char(36)", nullable: false),
                    storeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    contactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    openingHours = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stores", x => x.storeId);
                });

            migrationBuilder.CreateTable(
                name: "StoreImages",
                columns: table => new
                {
                    imageId = table.Column<string>(type: "char(36)", nullable: false),
                    storeId = table.Column<string>(type: "char(36)", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    isPrimary = table.Column<bool>(type: "bit", nullable: false),
                    displayOrder = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreImages", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_StoreImages_Stores_storeId",
                        column: x => x.storeId,
                        principalTable: "Stores",
                        principalColumn: "storeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoreImages_storeId",
                table: "StoreImages",
                column: "storeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoreImages");

            migrationBuilder.DropTable(
                name: "Stores");
        }
    }
}
