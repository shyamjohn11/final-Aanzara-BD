using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseLocationAndOrderTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "city",
                table: "Warehouses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "Warehouses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "Warehouses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pincode",
                table: "Warehouses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                table: "Warehouses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "courierName",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "currentLocation",
                table: "Orders",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dealerId",
                table: "Orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "estimatedDeliveryDate",
                table: "Orders",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fulfilledByWarehouseId",
                table: "Orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "trackingNumber",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "allocatedWarehouseId",
                table: "OrderItems",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dealerId",
                table: "OrderItems",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_dealerId",
                table: "Orders",
                column: "dealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_fulfilledByWarehouseId",
                table: "Orders",
                column: "fulfilledByWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_allocatedWarehouseId",
                table: "OrderItems",
                column: "allocatedWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_dealerId",
                table: "OrderItems",
                column: "dealerId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Dealers_dealerId",
                table: "OrderItems",
                column: "dealerId",
                principalTable: "Dealers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Warehouses_allocatedWarehouseId",
                table: "OrderItems",
                column: "allocatedWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "warehouseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Dealers_dealerId",
                table: "Orders",
                column: "dealerId",
                principalTable: "Dealers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_fulfilledByWarehouseId",
                table: "Orders",
                column: "fulfilledByWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "warehouseId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Dealers_dealerId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Warehouses_allocatedWarehouseId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Dealers_dealerId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_fulfilledByWarehouseId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_dealerId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_fulfilledByWarehouseId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_allocatedWarehouseId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_dealerId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "city",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "pincode",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "state",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "courierName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "currentLocation",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "dealerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "estimatedDeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "fulfilledByWarehouseId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "trackingNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "allocatedWarehouseId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "dealerId",
                table: "OrderItems");
        }
    }
}
