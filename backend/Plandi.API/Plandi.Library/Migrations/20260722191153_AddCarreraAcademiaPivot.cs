using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddCarreraAcademiaPivot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carrera_academias",
                columns: table => new
                {
                    CarreraId = table.Column<long>(type: "bigint", nullable: false),
                    AcademiaId = table.Column<long>(type: "bigint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrera_academias", x => new { x.CarreraId, x.AcademiaId });
                    table.ForeignKey(
                        name: "FK_carrera_academias_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carrera_academias_carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_carrera_academias_AcademiaId",
                table: "carrera_academias",
                column: "AcademiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrera_academias");
        }
    }
}
