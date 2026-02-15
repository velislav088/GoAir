using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoAir.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftToFlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AircraftId",
                table: "Flights",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AircraftId",
                table: "Flights",
                column: "AircraftId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flights_Aircrafts_AircraftId",
                table: "Flights",
                column: "AircraftId",
                principalTable: "Aircrafts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flights_Aircrafts_AircraftId",
                table: "Flights");

            migrationBuilder.DropIndex(
                name: "IX_Flights_AircraftId",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "AircraftId",
                table: "Flights");
        }
    }
}
