using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderPaymentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    invoiceId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    invoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    gstBreakdownJson = table.Column<string>(type: "json", nullable: true),
                    pdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    generatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.invoiceId);
                    table.ForeignKey(
                        name: "FK_Invoices_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderAddresses",
                columns: table => new
                {
                    orderAddressId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    recipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    recipientPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    addressLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    addressLine2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    pincode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderAddresses", x => x.orderAddressId);
                    table.ForeignKey(
                        name: "FK_OrderAddresses_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    historyId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    changedByUserId = table.Column<string>(type: "char(36)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    changedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.historyId);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Users_changedByUserId",
                        column: x => x.changedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    paymentId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    paymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    paymentGateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    gatewayTransactionId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "INR"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    failureReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    paidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.paymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SavedPaymentMethods",
                columns: table => new
                {
                    savedMethodId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    paymentMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    gatewayToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    displayLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    isDefault = table.Column<bool>(type: "bit", nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedPaymentMethods", x => x.savedMethodId);
                    table.ForeignKey(
                        name: "FK_SavedPaymentMethods_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    refundId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    paymentId = table.Column<string>(type: "char(36)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    gatewayRefundId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.refundId);
                    table.ForeignKey(
                        name: "FK_Refunds_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_paymentId",
                        column: x => x.paymentId,
                        principalTable: "Payments",
                        principalColumn: "paymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_orderId",
                table: "Invoices",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderAddresses_orderId",
                table: "OrderAddresses",
                column: "orderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_changedByUserId",
                table: "OrderStatusHistory",
                column: "changedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_orderId",
                table: "OrderStatusHistory",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_orderId",
                table: "Payments",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_userId",
                table: "Payments",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_orderId",
                table: "Refunds",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_paymentId",
                table: "Refunds",
                column: "paymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedPaymentMethods_userId",
                table: "SavedPaymentMethods",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "OrderAddresses");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "SavedPaymentMethods");

            migrationBuilder.DropTable(
                name: "Payments");
        }
    }
}
