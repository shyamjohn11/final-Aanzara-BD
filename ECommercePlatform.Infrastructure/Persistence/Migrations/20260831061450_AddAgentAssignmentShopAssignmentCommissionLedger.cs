using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentAssignmentShopAssignmentCommissionLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentAssignments",
                columns: table => new
                {
                    assignmentId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    assignedByAdminId = table.Column<string>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    assignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentAssignments", x => x.assignmentId);
                    table.ForeignKey(
                        name: "FK_AgentAssignments_Agents_agentId",
                        column: x => x.agentId,
                        principalTable: "Agents",
                        principalColumn: "agentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentAssignments_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentAssignments_Users_assignedByAdminId",
                        column: x => x.assignedByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentCommission",
                columns: table => new
                {
                    commissionId = table.Column<string>(type: "char(36)", nullable: false),
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    commissionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    value = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommission", x => x.commissionId);
                    table.ForeignKey(
                        name: "FK_AgentCommission_Agents_agentId",
                        column: x => x.agentId,
                        principalTable: "Agents",
                        principalColumn: "agentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopAssignments",
                columns: table => new
                {
                    assignmentId = table.Column<string>(type: "char(36)", nullable: false),
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    businessAccountId = table.Column<string>(type: "char(36)", nullable: false),
                    assignedByAdminId = table.Column<string>(type: "char(36)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    assignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAssignments", x => x.assignmentId);
                    table.ForeignKey(
                        name: "FK_ShopAssignments_Agents_agentId",
                        column: x => x.agentId,
                        principalTable: "Agents",
                        principalColumn: "agentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAssignments_BusinessAccounts_businessAccountId",
                        column: x => x.businessAccountId,
                        principalTable: "BusinessAccounts",
                        principalColumn: "businessAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAssignments_Users_assignedByAdminId",
                        column: x => x.assignedByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopLedger",
                columns: table => new
                {
                    ledgerId = table.Column<string>(type: "char(36)", nullable: false),
                    businessAccountId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopLedger", x => x.ledgerId);
                    table.ForeignKey(
                        name: "FK_ShopLedger_BusinessAccounts_businessAccountId",
                        column: x => x.businessAccountId,
                        principalTable: "BusinessAccounts",
                        principalColumn: "businessAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopLedger_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgentCommissionEarnings",
                columns: table => new
                {
                    earningId = table.Column<string>(type: "char(36)", nullable: false),
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    orderId = table.Column<string>(type: "char(36)", nullable: false),
                    commissionId = table.Column<string>(type: "char(36)", nullable: false),
                    amountEarned = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    createdAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentCommissionEarnings", x => x.earningId);
                    table.ForeignKey(
                        name: "FK_AgentCommissionEarnings_AgentCommission_commissionId",
                        column: x => x.commissionId,
                        principalTable: "AgentCommission",
                        principalColumn: "commissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionEarnings_Agents_agentId",
                        column: x => x.agentId,
                        principalTable: "Agents",
                        principalColumn: "agentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgentCommissionEarnings_Orders_orderId",
                        column: x => x.orderId,
                        principalTable: "Orders",
                        principalColumn: "orderId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentAssignments_agentId",
                table: "AgentAssignments",
                column: "agentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentAssignments_assignedByAdminId",
                table: "AgentAssignments",
                column: "assignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentAssignments_orderId",
                table: "AgentAssignments",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommission_agentId",
                table: "AgentCommission",
                column: "agentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionEarnings_agentId",
                table: "AgentCommissionEarnings",
                column: "agentId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionEarnings_commissionId",
                table: "AgentCommissionEarnings",
                column: "commissionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentCommissionEarnings_orderId",
                table: "AgentCommissionEarnings",
                column: "orderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAssignments_agentId_businessAccountId",
                table: "ShopAssignments",
                columns: new[] { "agentId", "businessAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopAssignments_assignedByAdminId",
                table: "ShopAssignments",
                column: "assignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAssignments_businessAccountId",
                table: "ShopAssignments",
                column: "businessAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopLedger_businessAccountId",
                table: "ShopLedger",
                column: "businessAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopLedger_orderId",
                table: "ShopLedger",
                column: "orderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentAssignments");

            migrationBuilder.DropTable(
                name: "AgentCommissionEarnings");

            migrationBuilder.DropTable(
                name: "ShopAssignments");

            migrationBuilder.DropTable(
                name: "ShopLedger");

            migrationBuilder.DropTable(
                name: "AgentCommission");
        }
    }
}
