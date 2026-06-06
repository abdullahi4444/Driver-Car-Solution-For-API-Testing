using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drive_Car_Solutions_API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnershipAndRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "VehicleServices",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Vehicles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "ServiceTypes",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Customers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Token = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExpiryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsRevoked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 1,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 2,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 3,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 4,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 5,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ServiceTypes",
                keyColumn: "ServiceTypeId",
                keyValue: 6,
                column: "ApplicationUserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ApplicationUserId",
                table: "RefreshTokens",
                column: "ApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "VehicleServices");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Customers");
        }
    }
}
