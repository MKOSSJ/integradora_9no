using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaneacionDeliveryDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLimiteEntregaPlaneaciones",
                table: "periodos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaLimiteEntregaPlaneaciones",
                table: "periodos");
        }
    }
}
