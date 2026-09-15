using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingDeliveryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryZones",
                columns: table => new
                {
                    zoneId = table.Column<string>(type: "char(36)", nullable: false),
                    zoneName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    pincodes = table.Column<string>(type: "text", nullable: true),
                    radiusKm = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryZones", x => x.zoneId);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    shipmentId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    courierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    trackingNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    shippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    estimatedDeliveryDate = table.Column<DateTime>(type: "date", nullable: true),
                    deliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.shipmentId);
                    table.ForeignKey(
                        name: "FK_Shipments_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryPartners",
                columns: table => new
                {
                    deliveryPartnerId = table.Column<string>(type: "char(36)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    vehicleType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    zoneId = table.Column<string>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryPartners", x => x.deliveryPartnerId);
                    table.ForeignKey(
                        name: "FK_DeliveryPartners_DeliveryZones_zoneId",
                        column: x => x.zoneId,
                        principalTable: "DeliveryZones",
                        principalColumn: "zoneId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryAssignments",
                columns: table => new
                {
                    assignmentId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    deliveryPartnerId = table.Column<string>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    assignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAssignments", x => x.assignmentId);
                    table.ForeignKey(
                        name: "FK_DeliveryAssignments_DeliveryPartners_deliveryPartnerId",
                        column: x => x.deliveryPartnerId,
                        principalTable: "DeliveryPartners",
                        principalColumn: "deliveryPartnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryAssignments_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAssignments_deliveryPartnerId",
                table: "DeliveryAssignments",
                column: "deliveryPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAssignments_orderId",
                table: "DeliveryAssignments",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryPartners_zoneId",
                table: "DeliveryPartners",
                column: "zoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_orderId",
                table: "Shipments",
                column: "orderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryAssignments");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "DeliveryPartners");

            migrationBuilder.DropTable(
                name: "DeliveryZones");
        }
    }
}
