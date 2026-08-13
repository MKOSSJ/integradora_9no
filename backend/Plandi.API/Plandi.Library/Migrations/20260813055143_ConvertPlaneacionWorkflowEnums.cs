using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class ConvertPlaneacionWorkflowEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_planeaciones_didacticas_academias_AcademiaId",
                table: "planeaciones_didacticas");

            migrationBuilder.DropForeignKey(
                name: "FK_planeaciones_didacticas_asignaturas_AsignaturaId",
                table: "planeaciones_didacticas");

            migrationBuilder.AddColumn<int>(
                name: "Rol",
                table: "academia_usuarios",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // The previous column stored enum names. Convert existing rows before it is removed.
            migrationBuilder.Sql("""
                UPDATE academia_usuarios
                SET Rol = CASE RolEnAcademia
                    WHEN 'Docente' THEN 1
                    WHEN 'Revisor' THEN 2
                    WHEN 'Director' THEN 3
                    ELSE 1
                END;
                """);

            migrationBuilder.DropColumn(name: "RolEnAcademia", table: "academia_usuarios");

            migrationBuilder.AddColumn<int>(
                name: "EstadoTemporal",
                table: "planeaciones_didacticas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // SQL Server cannot safely ALTER enum names (nvarchar) into int values.
            migrationBuilder.Sql("""
                UPDATE planeaciones_didacticas
                SET EstadoTemporal = CASE Estado
                    WHEN 'Creada' THEN 1
                    WHEN 'Borrador' THEN 1
                    WHEN 'EnProceso' THEN 2
                    WHEN 'EnRevision' THEN 3
                    WHEN 'CorreccionSolicitada' THEN 4
                    WHEN 'Aprobada' THEN 5
                    WHEN 'Rechazada' THEN 6
                    WHEN 'Finalizada' THEN 7
                    WHEN 'Generada' THEN 7
                    ELSE 1
                END;
                """);

            // The previous model already created this index on the string column.
            // SQL Server requires it to be removed before replacing the column.
            migrationBuilder.DropIndex(
                name: "IX_planeaciones_didacticas_Estado",
                table: "planeaciones_didacticas");
            migrationBuilder.DropColumn(name: "Estado", table: "planeaciones_didacticas");
            migrationBuilder.RenameColumn(name: "EstadoTemporal", table: "planeaciones_didacticas", newName: "Estado");
            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_Estado",
                table: "planeaciones_didacticas",
                column: "Estado");

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 2L },
                column: "Rol",
                value: 1);

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 3L },
                column: "Rol",
                value: 1);

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 4L },
                column: "Rol",
                value: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_planeaciones_didacticas_academias_AcademiaId",
                table: "planeaciones_didacticas",
                column: "AcademiaId",
                principalTable: "academias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_planeaciones_didacticas_asignaturas_AsignaturaId",
                table: "planeaciones_didacticas",
                column: "AsignaturaId",
                principalTable: "asignaturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_planeaciones_didacticas_academias_AcademiaId",
                table: "planeaciones_didacticas");

            migrationBuilder.DropForeignKey(
                name: "FK_planeaciones_didacticas_asignaturas_AsignaturaId",
                table: "planeaciones_didacticas");

            migrationBuilder.AddColumn<string>(
                name: "RolEnAcademia",
                table: "academia_usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE academia_usuarios
                SET RolEnAcademia = CASE Rol
                    WHEN 1 THEN 'Docente'
                    WHEN 2 THEN 'Revisor'
                    WHEN 3 THEN 'Director'
                    ELSE 'Docente'
                END;
                """);

            migrationBuilder.DropColumn(name: "Rol", table: "academia_usuarios");

            migrationBuilder.AddColumn<string>(
                name: "EstadoTemporal",
                table: "planeaciones_didacticas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Borrador");

            migrationBuilder.Sql("""
                UPDATE planeaciones_didacticas
                SET EstadoTemporal = CASE Estado
                    WHEN 1 THEN 'Borrador'
                    WHEN 2 THEN 'EnProceso'
                    WHEN 3 THEN 'EnRevision'
                    WHEN 4 THEN 'CorreccionSolicitada'
                    WHEN 5 THEN 'Aprobada'
                    WHEN 6 THEN 'Rechazada'
                    WHEN 7 THEN 'Finalizada'
                    ELSE 'Borrador'
                END;
                """);

            migrationBuilder.DropIndex(
                name: "IX_planeaciones_didacticas_Estado",
                table: "planeaciones_didacticas");
            migrationBuilder.DropColumn(name: "Estado", table: "planeaciones_didacticas");
            migrationBuilder.RenameColumn(name: "EstadoTemporal", table: "planeaciones_didacticas", newName: "Estado");
            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_Estado",
                table: "planeaciones_didacticas",
                column: "Estado");

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 2L },
                column: "RolEnAcademia",
                value: "Docente");

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 3L },
                column: "RolEnAcademia",
                value: "Docente");

            migrationBuilder.UpdateData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 4L },
                column: "RolEnAcademia",
                value: "Revisor");

            migrationBuilder.AddForeignKey(
                name: "FK_planeaciones_didacticas_academias_AcademiaId",
                table: "planeaciones_didacticas",
                column: "AcademiaId",
                principalTable: "academias",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_planeaciones_didacticas_asignaturas_AsignaturaId",
                table: "planeaciones_didacticas",
                column: "AsignaturaId",
                principalTable: "asignaturas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
