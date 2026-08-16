using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicPeriodLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Estado",
                table: "periodos",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "periodos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql("""
                DECLARE @HoyMexico date = CONVERT(date, SYSUTCDATETIME() AT TIME ZONE 'UTC' AT TIME ZONE 'Central Standard Time (Mexico)');
                UPDATE [periodos]
                SET [Estado] = CASE
                        WHEN CONVERT(date, [FechaFin]) < @HoyMexico THEN 3
                        WHEN CONVERT(date, [FechaInicio]) > @HoyMexico THEN 1
                        ELSE 2
                    END,
                    [FechaCierre] = CASE WHEN CONVERT(date, [FechaFin]) < @HoyMexico THEN SYSUTCDATETIME() ELSE NULL END
                WHERE [DeletedAt] IS NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_Estado",
                table: "periodos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_periodos_FechaFin",
                table: "periodos",
                column: "FechaFin");

            migrationBuilder.CreateIndex(
                name: "IX_periodos_FechaInicio",
                table: "periodos",
                column: "FechaInicio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_periodos_Estado",
                table: "periodos");

            migrationBuilder.DropIndex(
                name: "IX_periodos_FechaFin",
                table: "periodos");

            migrationBuilder.DropIndex(
                name: "IX_periodos_FechaInicio",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "periodos");
        }
    }
}
