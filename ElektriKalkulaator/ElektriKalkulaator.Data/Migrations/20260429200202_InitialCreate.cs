using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElektriKalkulaator.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculationRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WireCrossSectionMm2 = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BreakerAmperes = table.Column<int>(type: "int", nullable: false),
                    CircuitType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomsFrom = table.Column<int>(type: "int", nullable: false),
                    RoomsTo = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerboxCalculations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RulesApplied = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerboxCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerboxRequirements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuildingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoomCount = table.Column<int>(type: "int", nullable: false),
                    SocketCount = table.Column<int>(type: "int", nullable: false),
                    LightCount = table.Column<int>(type: "int", nullable: false),
                    SwitchCount = table.Column<int>(type: "int", nullable: false),
                    HasElectricStove = table.Column<bool>(type: "bit", nullable: false),
                    FloorCount = table.Column<int>(type: "int", nullable: true),
                    TotalAreaM2 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerboxRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerboxRequirements_PowerboxCalculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "PowerboxCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RatedCurrent = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Voltage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WireCrossSectionMm2 = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerboxComponents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CalculationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CircuitType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WireCrossSectionMm2 = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerboxComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerboxComponents_PowerboxCalculations_CalculationId",
                        column: x => x.CalculationId,
                        principalTable: "PowerboxCalculations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PowerboxComponents_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CalculationRules",
                columns: new[] { "Id", "BreakerAmperes", "BuildingType", "CircuitType", "CreatedAt", "ModifiedAt", "RoomsFrom", "RoomsTo", "RuleName", "WireCrossSectionMm2" },
                values: new object[,]
                {
                    { new Guid("33333333-0000-0000-0000-000000000001"), 10, "korterelamu", "lighting", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Korterelamu valgustus", 1.5m },
                    { new Guid("33333333-0000-0000-0000-000000000002"), 16, "korterelamu", "socket", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Korterelamu pistikud", 2.5m },
                    { new Guid("33333333-0000-0000-0000-000000000003"), 32, "korterelamu", "stove", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Korterelamu pliit", 6.0m },
                    { new Guid("33333333-0000-0000-0000-000000000004"), 10, "eramu", "lighting", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Eramu valgustus", 1.5m },
                    { new Guid("33333333-0000-0000-0000-000000000005"), 16, "eramu", "socket", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Eramu pistikud", 2.5m },
                    { new Guid("33333333-0000-0000-0000-000000000006"), 32, "eramu", "stove", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Eramu pliit", 6.0m },
                    { new Guid("33333333-0000-0000-0000-000000000007"), 10, "ärihoone", "lighting", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Ärihoone valgustus", 1.5m },
                    { new Guid("33333333-0000-0000-0000-000000000008"), 16, "ärihoone", "socket", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 999, "Ärihoone pistikud", 2.5m }
                });

            migrationBuilder.InsertData(
                table: "ProductCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "ModifiedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Automaatkaitselülitid (MCB) ABB, Schneider, Hager", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kaitselülitid" },
                    { new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NYM-J ja NYY kaablid 1.5mm², 2.5mm², 6mm²", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Juhtmed" },
                    { new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Rikkevoolukaitsmed (RCD/RCCB) 25A, 40A, 63A", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "RCD / Rikkevoolukaitsmeid" },
                    { new Guid("11111111-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ühenduskarbid ja klemmiribad", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Klemmid" },
                    { new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plastik ja metallist kilbi korpused 4-24 mooduli jaoks", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kilbi korpused" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Brand", "CategoryId", "CreatedAt", "Description", "ImagePath", "ModifiedAt", "Name", "Price", "RatedCurrent", "StockQuantity", "Voltage", "WireCrossSectionMm2" },
                values: new object[,]
                {
                    { new Guid("22222222-0000-0000-0000-000000000001"), "ABB", new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1-pooluseline kaitselüliti B10A, 6kA", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABB S201-B10 Kaitselüliti 10A", 8.50m, 10m, 150, 230m, 1.5m },
                    { new Guid("22222222-0000-0000-0000-000000000002"), "ABB", new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1-pooluseline kaitselüliti B16A, 6kA", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABB S201-B16 Kaitselüliti 16A", 9.20m, 16m, 200, 230m, 2.5m },
                    { new Guid("22222222-0000-0000-0000-000000000003"), "ABB", new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1-pooluseline kaitselüliti B32A, 6kA", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABB S201-B32 Kaitselüliti 32A", 12.80m, 32m, 80, 230m, 6.0m },
                    { new Guid("22222222-0000-0000-0000-000000000004"), "Schneider", new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1-pooluseline kaitselüliti B10A", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Schneider Easy9 B10A", 7.90m, 10m, 120, 230m, 1.5m },
                    { new Guid("22222222-0000-0000-0000-000000000005"), "Schneider", new Guid("11111111-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "1-pooluseline kaitselüliti B16A", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Schneider Easy9 B16A", 8.50m, 16m, 180, 230m, 2.5m },
                    { new Guid("22222222-0000-0000-0000-000000000006"), "Draka", new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Valgustuse juhe, müüakse meetrites", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NYM-J 3x1.5mm² kaabel (1m)", 1.20m, 10m, 5000, 230m, 1.5m },
                    { new Guid("22222222-0000-0000-0000-000000000007"), "Draka", new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pistikute juhe, müüakse meetrites", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NYM-J 3x2.5mm² kaabel (1m)", 1.85m, 16m, 5000, 230m, 2.5m },
                    { new Guid("22222222-0000-0000-0000-000000000008"), "Draka", new Guid("11111111-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pliidi juhe, müüakse meetrites", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NYM-J 3x6mm² kaabel (1m)", 3.60m, 32m, 2000, 230m, 6.0m },
                    { new Guid("22222222-0000-0000-0000-000000000009"), "ABB", new Guid("11111111-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2-pooluseline rikkevoolukaitsme 40A 30mA", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABB F202 AC-40/0.03 RCD 40A", 42.00m, 40m, 50, 230m, null },
                    { new Guid("22222222-0000-0000-0000-000000000010"), "ABB", new Guid("11111111-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pinna-paigaldusega kilbi korpus 12 moodulile", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ABB Mistral41F 12 mooduli kilp", 28.50m, 0m, 30, 230m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerboxComponents_CalculationId",
                table: "PowerboxComponents",
                column: "CalculationId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerboxComponents_ProductId",
                table: "PowerboxComponents",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerboxRequirements_CalculationId",
                table: "PowerboxRequirements",
                column: "CalculationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculationRules");

            migrationBuilder.DropTable(
                name: "PowerboxComponents");

            migrationBuilder.DropTable(
                name: "PowerboxRequirements");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "PowerboxCalculations");

            migrationBuilder.DropTable(
                name: "ProductCategories");
        }
    }
}
