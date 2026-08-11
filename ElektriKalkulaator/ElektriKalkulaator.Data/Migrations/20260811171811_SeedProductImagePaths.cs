using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElektriKalkulaator.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductImagePaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "ImagePath",
                value: "/images/products/breaker-10a.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "ImagePath",
                value: "/images/products/breaker-16a.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000003"),
                column: "ImagePath",
                value: "/images/products/breaker-32a.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "ImagePath",
                value: "/images/products/breaker-10a.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "ImagePath",
                value: "/images/products/breaker-16a.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "ImagePath",
                value: "/images/products/cable-1-5.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "ImagePath",
                value: "/images/products/cable-2-5.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "ImagePath",
                value: "/images/products/cable-6.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "ImagePath",
                value: "/images/products/rcd.svg");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"),
                column: "ImagePath",
                value: "/images/products/enclosure.svg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000001"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000002"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000003"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000004"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000005"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000006"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000007"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000008"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000009"),
                column: "ImagePath",
                value: null);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("22222222-0000-0000-0000-000000000010"),
                column: "ImagePath",
                value: null);
        }
    }
}
