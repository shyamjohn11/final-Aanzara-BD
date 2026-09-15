using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedesignProductSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Barcode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubCategoryId_Status_StockActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "UX_Products_SkuCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_productId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DealerPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DistributorPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "HsnCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaximumOrderQty",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinimumOrderQty",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RetailerPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SkuCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Products",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "SubCategoryId",
                table: "Products",
                newName: "subCategoryId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Products",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "Products",
                newName: "productName");

            migrationBuilder.RenameColumn(
                name: "Mrp",
                table: "Products",
                newName: "mrp");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Products",
                newName: "createdAt");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Products",
                newName: "categoryId");

            migrationBuilder.RenameColumn(
                name: "BrandId",
                table: "Products",
                newName: "brandId");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "Products",
                newName: "productId");

            migrationBuilder.RenameColumn(
                name: "TaxIncluded",
                table: "Products",
                newName: "isOrganic");

            migrationBuilder.RenameColumn(
                name: "StockActive",
                table: "Products",
                newName: "isGstFree");

            migrationBuilder.RenameColumn(
                name: "ProductCode",
                table: "Products",
                newName: "sku");

            migrationBuilder.RenameColumn(
                name: "OpeningStock",
                table: "Products",
                newName: "moq");

            migrationBuilder.RenameColumn(
                name: "GstPercentage",
                table: "Products",
                newName: "discount");

            migrationBuilder.RenameIndex(
                name: "UX_Products_ProductCode",
                table: "Products",
                newName: "UX_Products_Sku");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "ProductImages",
                newName: "updatedAt");

            migrationBuilder.RenameColumn(
                name: "imageId",
                table: "ProductImages",
                newName: "productImageId");

            migrationBuilder.AlterColumn<string>(
                name: "subCategoryId",
                table: "Products",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(36)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "productName",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<decimal>(
                name: "mrp",
                table: "Products",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "categoryId",
                table: "Products",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(36)");

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "Products",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

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
                name: "IX_Products_SubCategoryId",
                table: "Products",
                column: "subCategoryId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategories_subCategoryId",
                table: "Products",
                column: "subCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
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

            migrationBuilder.DropForeignKey(
                name: "FK_Products_SubCategories_subCategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SubCategoryId",
                table: "Products");

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
                name: "price",
                table: "Products");

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

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "Products",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "subCategoryId",
                table: "Products",
                newName: "SubCategoryId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Products",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "productName",
                table: "Products",
                newName: "ProductName");

            migrationBuilder.RenameColumn(
                name: "mrp",
                table: "Products",
                newName: "Mrp");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Products",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "createdAt",
                table: "Products",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "categoryId",
                table: "Products",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "brandId",
                table: "Products",
                newName: "BrandId");

            migrationBuilder.RenameColumn(
                name: "productId",
                table: "Products",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "sku",
                table: "Products",
                newName: "ProductCode");

            migrationBuilder.RenameColumn(
                name: "moq",
                table: "Products",
                newName: "OpeningStock");

            migrationBuilder.RenameColumn(
                name: "isOrganic",
                table: "Products",
                newName: "TaxIncluded");

            migrationBuilder.RenameColumn(
                name: "isGstFree",
                table: "Products",
                newName: "StockActive");

            migrationBuilder.RenameColumn(
                name: "discount",
                table: "Products",
                newName: "GstPercentage");

            migrationBuilder.RenameIndex(
                name: "UX_Products_Sku",
                table: "Products",
                newName: "UX_Products_ProductCode");

            migrationBuilder.RenameColumn(
                name: "updatedAt",
                table: "ProductImages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "productImageId",
                table: "ProductImages",
                newName: "imageId");

            migrationBuilder.AlterColumn<string>(
                name: "SubCategoryId",
                table: "Products",
                type: "char(36)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<decimal>(
                name: "Mrp",
                table: "Products",
                type: "decimal(18,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryId",
                table: "Products",
                type: "char(36)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DealerPrice",
                table: "Products",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DistributorPrice",
                table: "Products",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HsnCode",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumOrderQty",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinimumOrderQty",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchasePrice",
                table: "Products",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailerPrice",
                table: "Products",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SkuCode",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseId",
                table: "Products",
                type: "char(36)",
                nullable: true);

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
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SubCategoryId_Status_StockActive",
                table: "Products",
                columns: new[] { "SubCategoryId", "Status", "StockActive" });

            migrationBuilder.CreateIndex(
                name: "UX_Products_SkuCode",
                table: "Products",
                column: "SkuCode",
                unique: true,
                filter: "[SkuCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_productId",
                table: "ProductImages",
                column: "productId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Brands_BrandId",
                table: "Products",
                column: "BrandId",
                principalTable: "Brands",
                principalColumn: "brandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_SubCategories_SubCategoryId",
                table: "Products",
                column: "SubCategoryId",
                principalTable: "SubCategories",
                principalColumn: "SubCategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
