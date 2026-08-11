using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElektriKalkulaator.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEvsReferenceToCalculationRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvsReference",
                table: "CalculationRules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000001"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000002"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000003"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000004"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000005"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000006"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000007"),
                column: "EvsReference",
                value: null);

            migrationBuilder.UpdateData(
                table: "CalculationRules",
                keyColumn: "Id",
                keyValue: new Guid("33333333-0000-0000-0000-000000000008"),
                column: "EvsReference",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvsReference",
                table: "CalculationRules");
        }
    }
}
