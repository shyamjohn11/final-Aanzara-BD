using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInventoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    orderStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    itemsTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    discount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    gstAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    deliveryCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    handlingFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    grandTotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    appliedCouponId = table.Column<string>(type: "char(36)", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.orderId);
                    table.ForeignKey(
                        name: "FK_Orders_Coupons_appliedCouponId",
                        column: x => x.appliedCouponId,
                        principalTable: "Coupons",
                        principalColumn: "couponId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Orders_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    imageId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    imageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    isPrimary = table.Column<bool>(type: "bit", nullable: false),
                    displayOrder = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.imageId);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    warehouseId = table.Column<string>(type: "char(36)", nullable: false),
                    warehouseName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.warehouseId);
                });

            migrationBuilder.CreateTable(
                name: "WholesalePriceTiers",
                columns: table => new
                {
                    tierId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    minQuantity = table.Column<int>(type: "int", nullable: false),
                    maxQuantity = table.Column<int>(type: "int", nullable: true),
                    pricePerUnit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholesalePriceTiers", x => x.tierId);
                    table.ForeignKey(
                        name: "FK_WholesalePriceTiers_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    orderItemId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.orderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    inventoryId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    warehouseId = table.Column<string>(type: "char(36)", nullable: false),
                    stockQuantity = table.Column<int>(type: "int", nullable: false),
                    reservedQuantity = table.Column<int>(type: "int", nullable: false),
                    reorderLevel = table.Column<int>(type: "int", nullable: false),
                    dispatchEstimateDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.inventoryId);
                    table.ForeignKey(
                        name: "FK_Inventory_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventory_Warehouses_warehouseId",
                        column: x => x.warehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "warehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    movementId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    warehouseId = table.Column<string>(type: "char(36)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    orderId = table.Column<string>(type: "char(36)", nullable: true),
                    createdByUserId = table.Column<string>(type: "char(36)", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.movementId);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Users_createdByUserId",
                        column: x => x.createdByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Warehouses_warehouseId",
                        column: x => x.warehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "warehouseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_productId_warehouseId",
                table: "Inventory",
                columns: new[] { "productId", "warehouseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_warehouseId",
                table: "Inventory",
                column: "warehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_createdByUserId",
                table: "InventoryMovements",
                column: "createdByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_orderId",
                table: "InventoryMovements",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_productId",
                table: "InventoryMovements",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_warehouseId",
                table: "InventoryMovements",
                column: "warehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_orderId",
                table: "OrderItems",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_productId",
                table: "OrderItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_appliedCouponId",
                table: "Orders",
                column: "appliedCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_userId",
                table: "Orders",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_productId",
                table: "ProductImages",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_WholesalePriceTiers_productId",
                table: "WholesalePriceTiers",
                column: "productId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "WholesalePriceTiers");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
