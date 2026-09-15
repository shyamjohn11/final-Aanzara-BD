using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubCategoryImages",
                columns: table => new
                {
                    imageId = table.Column<string>(type: "char(36)", nullable: false),
                    subCategoryId = table.Column<string>(type: "char(36)", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    isPrimary = table.Column<bool>(type: "bit", nullable: false),
                    displayOrder = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategoryImages", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_SubCategoryImages_SubCategories_subCategoryId",
                        column: x => x.subCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "SubCategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryImages_subCategoryId",
                table: "SubCategoryImages",
                column: "subCategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubCategoryImages");
        }
    }
}
