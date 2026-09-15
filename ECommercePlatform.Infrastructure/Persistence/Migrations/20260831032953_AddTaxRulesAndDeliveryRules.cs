using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRulesAndDeliveryRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryRules",
                columns: table => new
                {
                    deliveryRuleId = table.Column<string>(type: "char(36)", nullable: false),
                    minOrderValueForFreeDelivery = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    flatDeliveryCharge = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    handlingFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRules", x => x.deliveryRuleId);
                });

            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    taxRuleId = table.Column<string>(type: "char(36)", nullable: false),
                    categoryId = table.Column<string>(type: "char(36)", nullable: true),
                    gstPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.taxRuleId);
                    table.ForeignKey(
                        name: "FK_TaxRules_Categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_categoryId",
                table: "TaxRules",
                column: "categoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryRules");

            migrationBuilder.DropTable(
                name: "TaxRules");
        }
    }
}
