using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubCategoryImageFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubCategoryImages_subCategoryId",
                table: "SubCategoryImages");

            migrationBuilder.AddColumn<string>(
                name: "contentType",
                table: "SubCategoryImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fileName",
                table: "SubCategoryImages",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "filePath",
                table: "SubCategoryImages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "fileSize",
                table: "SubCategoryImages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryImages_SubCategoryId_DisplayOrder",
                table: "SubCategoryImages",
                columns: new[] { "subCategoryId", "displayOrder" });

            migrationBuilder.CreateIndex(
                name: "UX_SubCategoryImages_SubCategoryId_Primary",
                table: "SubCategoryImages",
                column: "subCategoryId",
                unique: true,
                filter: "[isPrimary] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubCategoryImages_SubCategoryId_DisplayOrder",
                table: "SubCategoryImages");

            migrationBuilder.DropIndex(
                name: "UX_SubCategoryImages_SubCategoryId_Primary",
                table: "SubCategoryImages");

            migrationBuilder.DropColumn(
                name: "contentType",
                table: "SubCategoryImages");

            migrationBuilder.DropColumn(
                name: "fileName",
                table: "SubCategoryImages");

            migrationBuilder.DropColumn(
                name: "filePath",
                table: "SubCategoryImages");

            migrationBuilder.DropColumn(
                name: "fileSize",
                table: "SubCategoryImages");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryImages_subCategoryId",
                table: "SubCategoryImages",
                column: "subCategoryId");
        }
    }
}
