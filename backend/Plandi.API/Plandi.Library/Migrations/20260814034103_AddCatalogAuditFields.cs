using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "periodos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "periodos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "grupos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "grupos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "ciclos_escolares",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "ciclos_escolares",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "carreras",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "carreras",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "carga_academica",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "asignaturas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "asignaturas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "academias",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UpdatedBy",
                table: "academias",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "grupos");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "grupos");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ciclos_escolares");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ciclos_escolares");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "carreras");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "carreras");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "carga_academica");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "asignaturas");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "asignaturas");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "academias");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "academias");
        }
    }
}
