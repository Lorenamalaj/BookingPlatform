using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyAddressNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Properties_PropertyId1",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PropertyId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PropertyId1",
                table: "Bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId1",
                table: "Properties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_AddressId1",
                table: "Properties",
                column: "AddressId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Addresses_AddressId1",
                table: "Properties",
                column: "AddressId1",
                principalTable: "Addresses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Addresses_AddressId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_AddressId1",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AddressId1",
                table: "Properties");

            migrationBuilder.AddColumn<Guid>(
                name: "PropertyId1",
                table: "Bookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PropertyId1",
                table: "Bookings",
                column: "PropertyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Properties_PropertyId1",
                table: "Bookings",
                column: "PropertyId1",
                principalTable: "Properties",
                principalColumn: "Id");
        }
    }
}
