using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TestNoOp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_brandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_SubCategoryImages_SubCategoryId_DisplayOrder",
                table: "SubCategoryImages");

            migrationBuilder.DropIndex(
                name: "UX_SubCategoryImages_SubCategoryId_Primary",
                table: "SubCategoryImages");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubCategoryId_Status",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "UX_ProductImages_ProductId_Primary",
                table: "ProductImages");

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

            migrationBuilder.DropColumn(
                name: "contentType",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "fileName",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "filePath",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "fileSize",
                table: "ProductImages");

            migrationBuilder.RenameIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                newName: "IX_Products_brandId");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "ProductImages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "productImageId",
                table: "ProductImages",
                newName: "imageId");

            migrationBuilder.AlterColumn<string>(
                name: "imageUrl",
                table: "ProductImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "IX_SubCategoryImages_subCategoryId",
                table: "SubCategoryImages",
                column: "subCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_productId",
                table: "ProductImages",
                column: "productId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_brandId",
                table: "Products",
                column: "brandId",
                principalTable: "Brands",
                principalColumn: "brandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_brandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_SubCategoryImages_subCategoryId",
                table: "SubCategoryImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_productId",
                table: "ProductImages");

            migrationBuilder.RenameIndex(
                name: "IX_Products_brandId",
                table: "Products",
                newName: "IX_Products_BrandId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ProductImages",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "imageId",
                table: "ProductImages",
                newName: "productImageId");

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

            migrationBuilder.AlterColumn<string>(
                name: "imageUrl",
                table: "ProductImages",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "contentType",
                table: "ProductImages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "fileName",
                table: "ProductImages",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "filePath",
                table: "ProductImages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "fileSize",
                table: "ProductImages",
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

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId_Status",
                table: "Products",
                columns: new[] { "subCategoryId", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId_DisplayOrder",
                table: "ProductImages",
                columns: new[] { "productId", "displayOrder" });

            migrationBuilder.CreateIndex(
                name: "UX_ProductImages_ProductId_Primary",
                table: "ProductImages",
                column: "productId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_brandId",
                table: "Products",
                column: "brandId",
                principalTable: "Brands",
                principalColumn: "brandId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_categoryId",
                table: "Products",
                column: "categoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
