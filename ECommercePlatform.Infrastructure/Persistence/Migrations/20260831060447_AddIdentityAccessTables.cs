using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommercePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityAccessTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    addressId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    label = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    addressLine1 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    addressLine2 = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    state = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    pincode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: false),
                    isDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.addressId);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    agentId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    createdByAdminId = table.Column<string>(type: "char(36)", nullable: false),
                    assignedStoreId = table.Column<string>(type: "char(36)", nullable: true),
                    employeeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.agentId);
                    table.ForeignKey(
                        name: "FK_Agents_Users_createdByAdminId",
                        column: x => x.createdByAdminId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Agents_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessAccounts",
                columns: table => new
                {
                    businessAccountId = table.Column<string>(type: "char(36)", nullable: false),
                    userId = table.Column<string>(type: "char(36)", nullable: false),
                    companyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    shopName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ownerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    gstNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    documentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    verificationStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    creditLimit = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    currentBalance = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessAccounts", x => x.businessAccountId);
                    table.ForeignKey(
                        name: "FK_BusinessAccounts_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    permissionId = table.Column<string>(type: "char(36)", nullable: false),
                    permissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    module = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.permissionId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    roleId = table.Column<string>(type: "char(36)", nullable: false),
                    roleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.roleId);
                });

            migrationBuilder.CreateTable(
                name: "AdminUserRoles",
                columns: table => new
                {
                    adminUserId = table.Column<string>(type: "char(36)", nullable: false),
                    roleId = table.Column<string>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUserRoles", x => new { x.adminUserId, x.roleId });
                    table.ForeignKey(
                        name: "FK_AdminUserRoles_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminUserRoles_Users_adminUserId",
                        column: x => x.adminUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    roleId = table.Column<string>(type: "char(36)", nullable: false),
                    permissionId = table.Column<string>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.roleId, x.permissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_permissionId",
                        column: x => x.permissionId,
                        principalTable: "Permissions",
                        principalColumn: "permissionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_roleId",
                        column: x => x.roleId,
                        principalTable: "Roles",
                        principalColumn: "roleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_userId",
                table: "Addresses",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUserRoles_roleId",
                table: "AdminUserRoles",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_createdByAdminId",
                table: "Agents",
                column: "createdByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Agents_userId",
                table: "Agents",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessAccounts_userId",
                table: "BusinessAccounts",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_permissionId",
                table: "RolePermissions",
                column: "permissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AdminUserRoles");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "BusinessAccounts");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
