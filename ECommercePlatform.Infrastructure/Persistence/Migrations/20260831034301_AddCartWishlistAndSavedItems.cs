using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCartWishlistAndSavedItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    couponId = table.Column<string>(type: "char(36)", nullable: false),
                    couponCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    discountType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    discountValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    minOrderValue = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    usageLimitPerUser = table.Column<int>(type: "int", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.couponId);
                });

            migrationBuilder.CreateTable(
                name: "SavedItems",
                columns: table => new
                {
                    savedItemId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedItems", x => x.savedItemId);
                    table.ForeignKey(
                        name: "FK_SavedItems_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedItems_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    wishlistItemId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.wishlistItemId);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    cartId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    appliedCouponId = table.Column<string>(type: "char(36)", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.cartId);
                    table.ForeignKey(
                        name: "FK_Carts_Coupons_appliedCouponId",
                        column: x => x.appliedCouponId,
                        principalTable: "Coupons",
                        principalColumn: "couponId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Carts_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    cartItemId = table.Column<string>(type: "char(36)", nullable: false),
                    cartId = table.Column<string>(type: "char(36)", nullable: false),
                    productId = table.Column<string>(type: "char(36)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.cartItemId);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_cartId",
                        column: x => x.cartId,
                        principalTable: "Carts",
                        principalColumn: "cartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_Products_productId",
                        column: x => x.productId,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_cartId",
                table: "CartItems",
                column: "cartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_productId",
                table: "CartItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_appliedCouponId",
                table: "Carts",
                column: "appliedCouponId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_userId",
                table: "Carts",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_couponCode",
                table: "Coupons",
                column: "couponCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedItems_productId",
                table: "SavedItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedItems_userId",
                table: "SavedItems",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_productId",
                table: "WishlistItems",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_userId_productId",
                table: "WishlistItems",
                columns: new[] { "userId", "productId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "SavedItems");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Coupons");
        }
    }
}
