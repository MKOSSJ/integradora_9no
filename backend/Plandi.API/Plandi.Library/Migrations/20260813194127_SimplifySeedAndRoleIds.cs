using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySeedAndRoleIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 2L });

            migrationBuilder.DeleteData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 3L });

            migrationBuilder.DeleteData(
                table: "academia_usuarios",
                keyColumns: new[] { "AcademiaId", "UsuarioId" },
                keyValues: new object[] { 1L, 4L });

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "carga_academica",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "carga_academica",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "chat_mensajes",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "chat_mensajes",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "chat_participantes",
                keyColumns: new[] { "ChatId", "UsuarioId" },
                keyValues: new object[] { 1L, 2L });

            migrationBuilder.DeleteData(
                table: "chat_participantes",
                keyColumns: new[] { "ChatId", "UsuarioId" },
                keyValues: new object[] { 1L, 3L });

            migrationBuilder.DeleteData(
                table: "chat_participantes",
                keyColumns: new[] { "ChatId", "UsuarioId" },
                keyValues: new object[] { 1L, 4L });

            migrationBuilder.DeleteData(
                table: "planeacion_observaciones",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "planeacion_unidades",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "programas_asignatura",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "usuario_roles",
                keyColumns: new[] { "RolId", "UsuarioId" },
                keyValues: new object[] { 1L, 1L });

            migrationBuilder.DeleteData(
                table: "usuario_roles",
                keyColumns: new[] { "RolId", "UsuarioId" },
                keyValues: new object[] { 2L, 2L });

            migrationBuilder.DeleteData(
                table: "usuario_roles",
                keyColumns: new[] { "RolId", "UsuarioId" },
                keyValues: new object[] { 2L, 3L });

            migrationBuilder.DeleteData(
                table: "usuario_roles",
                keyColumns: new[] { "RolId", "UsuarioId" },
                keyValues: new object[] { 3L, 4L });

            // Remove the legacy role catalog before inserting the new stable IDs.
            // Updating Id=1 directly to "Docente" conflicts with the old Id=2
            // because roles.Nombre has a unique index.
            migrationBuilder.DeleteData(table: "roles", keyColumn: "Id", keyValue: 1L);
            migrationBuilder.DeleteData(table: "roles", keyColumn: "Id", keyValue: 2L);
            migrationBuilder.DeleteData(table: "roles", keyColumn: "Id", keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "academias",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "chats",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "documentos",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "grupos",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "grupos",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "planeacion_unidades",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "carreras",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "planeaciones_didacticas",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "asignaturas",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "periodos",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "academias",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ciclos_escolares",
                keyColumn: "Id",
                keyValue: 1L);

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "Descripcion", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Docente responsable de sus planeaciones.", "Docente", new Guid("10000000-0000-0000-0000-000000000001"), null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Usuario que revisa planeaciones asignadas.", "Revisor", new Guid("10000000-0000-0000-0000-000000000002"), null },
                    { 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Usuario que administra asignaciones y roles.", "Director", new Guid("10000000-0000-0000-0000-000000000003"), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "academias",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "Descripcion", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Academia relacionada con programación, bases de datos y desarrollo web", "Desarrollo de Software", new Guid("30000000-0000-0000-0000-000000000001"), null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Academia de asignaturas de inglés", "Inglés", new Guid("30000000-0000-0000-0000-000000000002"), null }
                });

            migrationBuilder.InsertData(
                table: "carreras",
                columns: new[] { "Id", "Activo", "Clave", "CreatedAt", "DeletedAt", "Nivel", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[] { 1L, true, "ITI", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Ingeniería", "Ingeniería en Tecnologías de la Información e Innovación Digital", new Guid("40000000-0000-0000-0000-000000000001"), null });

            migrationBuilder.InsertData(
                table: "ciclos_escolares",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "FechaFin", "FechaInicio", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[] { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2027, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "2026-2027", new Guid("50000000-0000-0000-0000-000000000001"), null });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Usuario con acceso total al sistema", "Administrador" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Profesor que participa en planeaciones didácticas", "Docente" });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Usuario encargado de revisar planeaciones didácticas", "Revisor" });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "Descripcion", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[] { 4L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Usuario encargado de aprobación final", "Director", new Guid("10000000-0000-0000-0000-000000000004"), null });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "Id", "AccessFailedCount", "Activo", "ApellidoMaterno", "ApellidoPaterno", "CreatedAt", "DeletedAt", "Email", "LockoutEnd", "Nombre", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpires", "PublicId", "Telefono", "TwoFactorEnabled", "TwoFactorSecretKey", "UltimoAcceso", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 0, true, null, "Sistema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@uth.edu.mx", null, "Admin", "DEV_HASH_SOLO_PRUEBAS", null, null, new Guid("20000000-0000-0000-0000-000000000001"), null, false, null, null, null },
                    { 2L, 0, true, null, "Pérez", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "juan.perez@uth.edu.mx", null, "Juan", "DEV_HASH_SOLO_PRUEBAS", null, null, new Guid("20000000-0000-0000-0000-000000000002"), null, false, null, null, null },
                    { 3L, 0, true, null, "Torres", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ana.torres@uth.edu.mx", null, "Ana", "DEV_HASH_SOLO_PRUEBAS", null, null, new Guid("20000000-0000-0000-0000-000000000003"), null, false, null, null, null },
                    { 4L, 0, true, null, "López", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "maria.lopez@uth.edu.mx", null, "María", "DEV_HASH_SOLO_PRUEBAS", null, null, new Guid("20000000-0000-0000-0000-000000000004"), null, false, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "academia_usuarios",
                columns: new[] { "AcademiaId", "UsuarioId", "Activo", "CreatedAt", "Rol" },
                values: new object[,]
                {
                    { 1L, 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 1L, 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 1L, 4L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 }
                });

            migrationBuilder.InsertData(
                table: "asignaturas",
                columns: new[] { "Id", "AcademiaId", "Activo", "Clave", "CreatedAt", "Creditos", "Cuatrimestre", "DeletedAt", "HorasSemana", "HorasTotales", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, true, "AW-701", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5m, 7, null, 5, 75, "Aplicaciones Web", new Guid("80000000-0000-0000-0000-000000000001"), null },
                    { 2L, 2L, true, "ING-701", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4m, 7, null, 4, 60, "Inglés VII", new Guid("80000000-0000-0000-0000-000000000002"), null }
                });

            migrationBuilder.InsertData(
                table: "documentos",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "Estado", "Extension", "FechaSubida", "HashSha256", "MimeType", "NombreGuardado", "NombreOriginal", "PublicId", "RutaStorage", "SubidoPorId", "TamanoBytes", "TipoDocumento", "Titulo", "UpdatedAt" },
                values: new object[] { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Procesado", ".pdf", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", "application/pdf", "programa-aplicaciones-web-dev.pdf", "programa-aplicaciones-web.pdf", new Guid("aaaaaaaa-0000-0000-0000-000000000001"), "documentos/programas-asignatura/2026/programa-aplicaciones-web-dev.pdf", 1L, 204800L, "ProgramaAsignatura", "Programa de Asignatura - Aplicaciones Web", null });

            migrationBuilder.InsertData(
                table: "periodos",
                columns: new[] { "Id", "Activo", "CicloEscolarId", "CreatedAt", "DeletedAt", "FechaFin", "FechaInicio", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[] { 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 12, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Septiembre-Diciembre 2026", new Guid("60000000-0000-0000-0000-000000000001"), null });

            migrationBuilder.InsertData(
                table: "usuario_roles",
                columns: new[] { "RolId", "UsuarioId", "CreatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2L, 2L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2L, 3L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3L, 4L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "grupos",
                columns: new[] { "Id", "Activo", "CarreraId", "CreatedAt", "Cuatrimestre", "DeletedAt", "Nombre", "PeriodoId", "PublicId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, null, "ITI-701", 1L, new Guid("70000000-0000-0000-0000-000000000001"), null },
                    { 2L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, null, "ITI-702", 1L, new Guid("70000000-0000-0000-0000-000000000002"), null }
                });

            migrationBuilder.InsertData(
                table: "planeaciones_didacticas",
                columns: new[] { "Id", "AcademiaId", "Activo", "AsignaturaId", "CreatedAt", "CreatedBy", "DeletedAt", "Estado", "FechaUltimaModificacion", "PeriodoId", "PublicId", "RevisorId", "UltimaModificacionPorId", "UpdatedAt" },
                values: new object[] { 1L, null, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, null, 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, new Guid("cccccccc-0000-0000-0000-000000000001"), 4L, 2L, null });

            migrationBuilder.InsertData(
                table: "programas_asignatura",
                columns: new[] { "Id", "AcademiaId", "Activo", "AsignaturaId", "Carrera", "ClaveAsignatura", "Competencia", "CreatedAt", "Creditos", "Cuatrimestre", "DeletedAt", "DocumentoId", "FechaUltimaModificacion", "HorasSemana", "HorasTotales", "JsonExtraido", "NombreAsignatura", "Proposito", "PublicId", "TextoExtraido", "UltimaModificacionPorId", "UpdatedAt" },
                values: new object[] { 1L, 1L, true, 1L, "Ingeniería en Tecnologías de la Información e Innovación Digital", "AW-701", "Desarrollar aplicaciones web utilizando tecnologías actuales y buenas prácticas de programación.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5m, 7, null, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 75, "{\r\n  \"unidades\": [\r\n    {\r\n      \"numero\": \"I\",\r\n      \"nombre\": \"Introducción a las aplicaciones web\",\r\n      \"resultado_aprendizaje\": \"El alumno identificará los componentes básicos de una aplicación web.\",\r\n      \"temas\": [\r\n        \"Cliente-servidor\",\r\n        \"HTTP\",\r\n        \"APIs REST\"\r\n      ]\r\n    },\r\n    {\r\n      \"numero\": \"II\",\r\n      \"nombre\": \"Desarrollo de APIs\",\r\n      \"resultado_aprendizaje\": \"El alumno desarrollará servicios web usando arquitectura por capas.\",\r\n      \"temas\": [\r\n        \"Controladores\",\r\n        \"Servicios\",\r\n        \"DTOs\",\r\n        \"Entity Framework Core\"\r\n      ]\r\n    }\r\n  ]\r\n}", "Aplicaciones Web", "El alumno desarrollará aplicaciones web funcionales aplicando arquitectura por capas.", new Guid("bbbbbbbb-0000-0000-0000-000000000001"), "Texto extraído de prueba del programa de asignatura.", 1L, null });

            migrationBuilder.InsertData(
                table: "carga_academica",
                columns: new[] { "Id", "AcademiaId", "Activo", "AsignaturaId", "CreatedAt", "CreatedBy", "DeletedAt", "DocenteId", "GrupoId", "PeriodoId", "PublicId", "RevisorId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, null, 2L, 1L, 1L, new Guid("90000000-0000-0000-0000-000000000001"), 4L, null },
                    { 2L, 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, null, 3L, 2L, 1L, new Guid("90000000-0000-0000-0000-000000000002"), 4L, null }
                });

            migrationBuilder.InsertData(
                table: "chats",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "PlaneacionDidacticaId", "PublicId", "Titulo", "UpdatedAt" },
                values: new object[] { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1L, new Guid("ffffffff-0000-0000-0000-000000000001"), "Chat - Planeación Aplicaciones Web", null });

            migrationBuilder.InsertData(
                table: "planeacion_unidades",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "FechaUltimaModificacion", "HorasSaber", "HorasSaberHacer", "HorasTotales", "NombreUnidad", "NumeroUnidad", "Orden", "PlaneacionDidacticaId", "PorcentajeUnidad", "PropositoEsperado", "PublicId", "UltimaModificacionPorId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 20, "Introducción a las aplicaciones web", 1, 1, 1L, null, "El alumno identificará los componentes básicos de una aplicación web.", new Guid("dddddddd-0000-0000-0000-000000000001"), 2L, null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, 30, "Desarrollo de APIs", 2, 2, 1L, null, "El alumno desarrollará servicios web usando arquitectura por capas.", new Guid("dddddddd-0000-0000-0000-000000000002"), 3L, null }
                });

            migrationBuilder.InsertData(
                table: "chat_mensajes",
                columns: new[] { "Id", "Activo", "ChatId", "CreatedAt", "DeletedAt", "EditadoAt", "EliminadoAt", "Mensaje", "PublicId", "TipoMensaje", "UpdatedAt", "UsuarioId" },
                values: new object[,]
                {
                    { 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "Favor de revisar que las actividades correspondan con los resultados de aprendizaje.", new Guid("11111111-aaaa-0000-0000-000000000001"), "OBSERVACION", null, 4L },
                    { 2L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, null, "De acuerdo, revisaremos la Unidad I y ajustaremos las evidencias.", new Guid("11111111-aaaa-0000-0000-000000000002"), "TEXTO", null, 2L }
                });

            migrationBuilder.InsertData(
                table: "chat_participantes",
                columns: new[] { "ChatId", "UsuarioId", "Activo", "CreatedAt", "RolEnChat" },
                values: new object[,]
                {
                    { 1L, 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DOCENTE" },
                    { 1L, 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DOCENTE" },
                    { 1L, 4L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "REVISOR" }
                });

            migrationBuilder.InsertData(
                table: "planeacion_observaciones",
                columns: new[] { "Id", "Activo", "Comentario", "CreatedAt", "DeletedAt", "Estado", "FechaAtendida", "FechaRevision", "PlaneacionDidacticaId", "PlaneacionEvaluacionId", "PlaneacionSecuenciaId", "PlaneacionTemaId", "PlaneacionUnidadId", "PublicId", "RevisorId", "Seccion", "UpdatedAt" },
                values: new object[] { 1L, true, "La evidencia de la Unidad I debe estar mejor relacionada con el resultado de aprendizaje.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ABIERTA", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, null, null, null, 1L, new Guid("22222222-aaaa-0000-0000-000000000001"), 4L, "Unidad 1 ", null });
        }
    }
}
