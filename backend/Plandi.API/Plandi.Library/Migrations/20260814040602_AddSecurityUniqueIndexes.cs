using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roles_Nombre",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_periodos_CicloEscolarId_Nombre",
                table: "periodos");

            migrationBuilder.DropIndex(
                name: "IX_grupos_PeriodoId_Nombre",
                table: "grupos");

            migrationBuilder.DropIndex(
                name: "IX_ciclos_escolares_Nombre",
                table: "ciclos_escolares");

            migrationBuilder.DropIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats");

            migrationBuilder.DropIndex(
                name: "IX_carreras_Clave",
                table: "carreras");

            migrationBuilder.DropIndex(
                name: "IX_carga_academica_PeriodoId_GrupoId_AsignaturaId_DocenteId",
                table: "carga_academica");

            migrationBuilder.DropIndex(
                name: "IX_asignaturas_Clave",
                table: "asignaturas");

            migrationBuilder.DropIndex(
                name: "IX_academias_Nombre",
                table: "academias");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                column: "Nombre",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_periodos_CicloEscolarId_Nombre",
                table: "periodos",
                columns: new[] { "CicloEscolarId", "Nombre" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_PeriodoId_Nombre",
                table: "grupos",
                columns: new[] { "PeriodoId", "Nombre" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ciclos_escolares_Nombre",
                table: "ciclos_escolares",
                column: "Nombre",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_chats_PlaneacionDidacticaId_Titulo",
                table: "chats",
                columns: new[] { "PlaneacionDidacticaId", "Titulo" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_carreras_Clave",
                table: "carreras",
                column: "Clave",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_PeriodoId_GrupoId_AsignaturaId_DocenteId",
                table: "carga_academica",
                columns: new[] { "PeriodoId", "GrupoId", "AsignaturaId", "DocenteId" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_Clave",
                table: "asignaturas",
                column: "Clave",
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_academias_Nombre",
                table: "academias",
                column: "Nombre",
                unique: true,
                filter: "[DeletedAt] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roles_Nombre",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_periodos_CicloEscolarId_Nombre",
                table: "periodos");

            migrationBuilder.DropIndex(
                name: "IX_grupos_PeriodoId_Nombre",
                table: "grupos");

            migrationBuilder.DropIndex(
                name: "IX_ciclos_escolares_Nombre",
                table: "ciclos_escolares");

            migrationBuilder.DropIndex(
                name: "IX_chats_PlaneacionDidacticaId_Titulo",
                table: "chats");

            migrationBuilder.DropIndex(
                name: "IX_carreras_Clave",
                table: "carreras");

            migrationBuilder.DropIndex(
                name: "IX_carga_academica_PeriodoId_GrupoId_AsignaturaId_DocenteId",
                table: "carga_academica");

            migrationBuilder.DropIndex(
                name: "IX_asignaturas_Clave",
                table: "asignaturas");

            migrationBuilder.DropIndex(
                name: "IX_academias_Nombre",
                table: "academias");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_CicloEscolarId_Nombre",
                table: "periodos",
                columns: new[] { "CicloEscolarId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grupos_PeriodoId_Nombre",
                table: "grupos",
                columns: new[] { "PeriodoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ciclos_escolares_Nombre",
                table: "ciclos_escolares",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats",
                column: "PlaneacionDidacticaId");

            migrationBuilder.CreateIndex(
                name: "IX_carreras_Clave",
                table: "carreras",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_PeriodoId_GrupoId_AsignaturaId_DocenteId",
                table: "carga_academica",
                columns: new[] { "PeriodoId", "GrupoId", "AsignaturaId", "DocenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_Clave",
                table: "asignaturas",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_academias_Nombre",
                table: "academias",
                column: "Nombre",
                unique: true);
        }
    }
}
