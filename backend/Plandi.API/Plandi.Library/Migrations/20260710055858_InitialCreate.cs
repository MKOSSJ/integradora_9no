using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academias",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "carreras",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carreras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ciclos_escolares",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciclos_escolares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoPaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApellidoMaterno = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    UltimoAcceso = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "asignaturas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AcademiaId = table.Column<long>(type: "bigint", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cuatrimestre = table.Column<int>(type: "int", nullable: false),
                    HorasTotales = table.Column<int>(type: "int", nullable: false),
                    HorasSemana = table.Column<int>(type: "int", nullable: false),
                    Creditos = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asignaturas_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "periodos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CicloEscolarId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_periodos_ciclos_escolares_CicloEscolarId",
                        column: x => x.CicloEscolarId,
                        principalTable: "ciclos_escolares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "academia_usuarios",
                columns: table => new
                {
                    AcademiaId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    RolEnAcademia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academia_usuarios", x => new { x.AcademiaId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_academia_usuarios_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_academia_usuarios_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documentos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NombreGuardado = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Extension = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    RutaStorage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    HashSha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SubidoPorId = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_documentos_usuarios_SubidoPorId",
                        column: x => x.SubidoPorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_roles",
                columns: table => new
                {
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    RolId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_roles", x => new { x.UsuarioId, x.RolId });
                    table.ForeignKey(
                        name: "FK_usuario_roles_roles_RolId",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_roles_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grupos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cuatrimestre = table.Column<int>(type: "int", nullable: false),
                    CarreraId = table.Column<long>(type: "bigint", nullable: false),
                    PeriodoId = table.Column<long>(type: "bigint", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grupos_carreras_CarreraId",
                        column: x => x.CarreraId,
                        principalTable: "carreras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_grupos_periodos_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "periodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "programas_asignatura",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoId = table.Column<long>(type: "bigint", nullable: false),
                    AsignaturaId = table.Column<long>(type: "bigint", nullable: true),
                    AcademiaId = table.Column<long>(type: "bigint", nullable: true),
                    NombreAsignatura = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClaveAsignatura = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Carrera = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cuatrimestre = table.Column<int>(type: "int", nullable: true),
                    Competencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Proposito = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Creditos = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    HorasTotales = table.Column<int>(type: "int", nullable: true),
                    HorasSemana = table.Column<int>(type: "int", nullable: true),
                    TextoExtraido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonExtraido = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UltimaModificacionPorId = table.Column<long>(type: "bigint", nullable: true),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programas_asignatura", x => x.Id);
                    table.ForeignKey(
                        name: "FK_programas_asignatura_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programas_asignatura_asignaturas_AsignaturaId",
                        column: x => x.AsignaturaId,
                        principalTable: "asignaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programas_asignatura_documentos_DocumentoId",
                        column: x => x.DocumentoId,
                        principalTable: "documentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_programas_asignatura_usuarios_UltimaModificacionPorId",
                        column: x => x.UltimaModificacionPorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "carga_academica",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodoId = table.Column<long>(type: "bigint", nullable: false),
                    GrupoId = table.Column<long>(type: "bigint", nullable: false),
                    AsignaturaId = table.Column<long>(type: "bigint", nullable: false),
                    DocenteId = table.Column<long>(type: "bigint", nullable: false),
                    RevisorId = table.Column<long>(type: "bigint", nullable: true),
                    AcademiaId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carga_academica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_carga_academica_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carga_academica_asignaturas_AsignaturaId",
                        column: x => x.AsignaturaId,
                        principalTable: "asignaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carga_academica_grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carga_academica_periodos_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "periodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carga_academica_usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_carga_academica_usuarios_RevisorId",
                        column: x => x.RevisorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeaciones_didacticas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodoId = table.Column<long>(type: "bigint", nullable: false),
                    AsignaturaId = table.Column<long>(type: "bigint", nullable: false),
                    AcademiaId = table.Column<long>(type: "bigint", nullable: true),
                    ProgramaAsignaturaId = table.Column<long>(type: "bigint", nullable: true),
                    RevisorId = table.Column<long>(type: "bigint", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UltimaModificacionPorId = table.Column<long>(type: "bigint", nullable: true),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeaciones_didacticas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_academias_AcademiaId",
                        column: x => x.AcademiaId,
                        principalTable: "academias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_asignaturas_AsignaturaId",
                        column: x => x.AsignaturaId,
                        principalTable: "asignaturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_periodos_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "periodos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_programas_asignatura_ProgramaAsignaturaId",
                        column: x => x.ProgramaAsignaturaId,
                        principalTable: "programas_asignatura",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_usuarios_RevisorId",
                        column: x => x.RevisorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeaciones_didacticas_usuarios_UltimaModificacionPorId",
                        column: x => x.UltimaModificacionPorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionDidacticaId = table.Column<long>(type: "bigint", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chats_planeaciones_didacticas_PlaneacionDidacticaId",
                        column: x => x.PlaneacionDidacticaId,
                        principalTable: "planeaciones_didacticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_docentes",
                columns: table => new
                {
                    PlaneacionDidacticaId = table.Column<long>(type: "bigint", nullable: false),
                    DocenteId = table.Column<long>(type: "bigint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_docentes", x => new { x.PlaneacionDidacticaId, x.DocenteId });
                    table.ForeignKey(
                        name: "FK_planeacion_docentes_planeaciones_didacticas_PlaneacionDidacticaId",
                        column: x => x.PlaneacionDidacticaId,
                        principalTable: "planeaciones_didacticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeacion_docentes_usuarios_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_grupos",
                columns: table => new
                {
                    PlaneacionDidacticaId = table.Column<long>(type: "bigint", nullable: false),
                    GrupoId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_grupos", x => new { x.PlaneacionDidacticaId, x.GrupoId });
                    table.ForeignKey(
                        name: "FK_planeacion_grupos_grupos_GrupoId",
                        column: x => x.GrupoId,
                        principalTable: "grupos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeacion_grupos_planeaciones_didacticas_PlaneacionDidacticaId",
                        column: x => x.PlaneacionDidacticaId,
                        principalTable: "planeaciones_didacticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_unidades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionDidacticaId = table.Column<long>(type: "bigint", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ResultadoAprendizaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Horas = table.Column<int>(type: "int", nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    UltimaModificacionPorId = table.Column<long>(type: "bigint", nullable: true),
                    FechaUltimaModificacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_unidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeacion_unidades_planeaciones_didacticas_PlaneacionDidacticaId",
                        column: x => x.PlaneacionDidacticaId,
                        principalTable: "planeaciones_didacticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeacion_unidades_usuarios_UltimaModificacionPorId",
                        column: x => x.UltimaModificacionPorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chat_mensajes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoMensaje = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EditadoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_mensajes_chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chat_mensajes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "chat_participantes",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    RolEnChat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_participantes", x => new { x.ChatId, x.UsuarioId });
                    table.ForeignKey(
                        name: "FK_chat_participantes_chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chat_participantes_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_actividades",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionUnidadId = table.Column<long>(type: "bigint", nullable: false),
                    TipoActividad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Semana = table.Column<int>(type: "int", nullable: true),
                    Horas = table.Column<int>(type: "int", nullable: true),
                    EstrategiaEnsenanza = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstrategiaAprendizaje = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Evidencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstrumentoEvaluacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PorcentajeEvaluacion = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_actividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeacion_actividades_planeacion_unidades_PlaneacionUnidadId",
                        column: x => x.PlaneacionUnidadId,
                        principalTable: "planeacion_unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_observaciones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionDidacticaId = table.Column<long>(type: "bigint", nullable: false),
                    PlaneacionUnidadId = table.Column<long>(type: "bigint", nullable: true),
                    RevisorId = table.Column<long>(type: "bigint", nullable: false),
                    Comentario = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_observaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeacion_observaciones_planeacion_unidades_PlaneacionUnidadId",
                        column: x => x.PlaneacionUnidadId,
                        principalTable: "planeacion_unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeacion_observaciones_planeaciones_didacticas_PlaneacionDidacticaId",
                        column: x => x.PlaneacionDidacticaId,
                        principalTable: "planeaciones_didacticas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planeacion_observaciones_usuarios_RevisorId",
                        column: x => x.RevisorId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "Descripcion", "Nombre", "PublicId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Usuario con acceso total al sistema", "Administrador", new Guid("10000000-0000-0000-0000-000000000001"), null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Profesor que participa en planeaciones didácticas", "Docente", new Guid("10000000-0000-0000-0000-000000000002"), null },
                    { 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Usuario encargado de revisar planeaciones didácticas", "Revisor", new Guid("10000000-0000-0000-0000-000000000003"), null },
                    { 4L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "Usuario encargado de aprobación final", "Director", new Guid("10000000-0000-0000-0000-000000000004"), null }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "Id", "Activo", "ApellidoMaterno", "ApellidoPaterno", "CreatedAt", "DeletedAt", "Email", "Nombre", "PasswordHash", "PublicId", "Telefono", "UltimoAcceso", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, null, "Sistema", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "admin@uth.edu.mx", "Admin", "DEV_HASH_SOLO_PRUEBAS", new Guid("20000000-0000-0000-0000-000000000001"), null, null, null },
                    { 2L, true, null, "Pérez", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "juan.perez@uth.edu.mx", "Juan", "DEV_HASH_SOLO_PRUEBAS", new Guid("20000000-0000-0000-0000-000000000002"), null, null, null },
                    { 3L, true, null, "Torres", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ana.torres@uth.edu.mx", "Ana", "DEV_HASH_SOLO_PRUEBAS", new Guid("20000000-0000-0000-0000-000000000003"), null, null, null },
                    { 4L, true, null, "López", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "maria.lopez@uth.edu.mx", "María", "DEV_HASH_SOLO_PRUEBAS", new Guid("20000000-0000-0000-0000-000000000004"), null, null, null }
                });

            migrationBuilder.InsertData(
                table: "academia_usuarios",
                columns: new[] { "AcademiaId", "UsuarioId", "Activo", "CreatedAt", "RolEnAcademia" },
                values: new object[,]
                {
                    { 1L, 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Docente" },
                    { 1L, 3L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Docente" },
                    { 1L, 4L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Revisor" }
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
                table: "planeaciones_didacticas",
                columns: new[] { "Id", "AcademiaId", "Activo", "AsignaturaId", "CreatedAt", "CreatedBy", "DeletedAt", "Estado", "FechaUltimaModificacion", "PeriodoId", "ProgramaAsignaturaId", "PublicId", "RevisorId", "Titulo", "UltimaModificacionPorId", "UpdatedAt" },
                values: new object[] { 1L, 1L, true, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, null, "EnProceso", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1L, 1L, new Guid("cccccccc-0000-0000-0000-000000000001"), 4L, "Planeación Didáctica - Aplicaciones Web - Septiembre-Diciembre 2026", 2L, null });

            migrationBuilder.InsertData(
                table: "chats",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "PlaneacionDidacticaId", "PublicId", "Titulo", "UpdatedAt" },
                values: new object[] { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1L, new Guid("ffffffff-0000-0000-0000-000000000001"), "Chat - Planeación Aplicaciones Web", null });

            migrationBuilder.InsertData(
                table: "planeacion_docentes",
                columns: new[] { "DocenteId", "PlaneacionDidacticaId", "Activo", "CreatedAt" },
                values: new object[,]
                {
                    { 2L, 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3L, 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "planeacion_grupos",
                columns: new[] { "GrupoId", "PlaneacionDidacticaId", "CreatedAt" },
                values: new object[,]
                {
                    { 1L, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2L, 1L, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "planeacion_unidades",
                columns: new[] { "Id", "Activo", "CreatedAt", "DeletedAt", "FechaUltimaModificacion", "Horas", "Nombre", "Numero", "Orden", "PlaneacionDidacticaId", "PublicId", "ResultadoAprendizaje", "UltimaModificacionPorId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 20, "Introducción a las aplicaciones web", "I", 1, 1L, new Guid("dddddddd-0000-0000-0000-000000000001"), "El alumno identificará los componentes básicos de una aplicación web.", 2L, null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 30, "Desarrollo de APIs", "II", 2, 1L, new Guid("dddddddd-0000-0000-0000-000000000002"), "El alumno desarrollará servicios web usando arquitectura por capas.", 3L, null }
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
                table: "planeacion_actividades",
                columns: new[] { "Id", "Activo", "CreatedAt", "CreatedBy", "DeletedAt", "Descripcion", "EstrategiaAprendizaje", "EstrategiaEnsenanza", "Evidencia", "Horas", "InstrumentoEvaluacion", "Orden", "PlaneacionUnidadId", "PorcentajeEvaluacion", "PublicId", "Semana", "TipoActividad", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2L, null, "Presentación de conceptos básicos de aplicaciones web.", "Discusión grupal y análisis de ejemplos.", "Exposición guiada y preguntas detonadoras.", "Mapa conceptual de arquitectura web.", 2, "Lista de cotejo", 1, 1L, 10m, new Guid("eeeeeeee-0000-0000-0000-000000000001"), 1, "APERTURA", null, null },
                    { 2L, true, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3L, null, "Construcción de una API REST con .NET por capas.", "Desarrollo guiado de endpoints.", "Demostración práctica.", "API funcional con controladores y servicios.", 6, "Rúbrica", 1, 2L, 30m, new Guid("eeeeeeee-0000-0000-0000-000000000002"), 4, "DESARROLLO", null, null }
                });

            migrationBuilder.InsertData(
                table: "planeacion_observaciones",
                columns: new[] { "Id", "Activo", "Comentario", "CreatedAt", "DeletedAt", "Estado", "PlaneacionDidacticaId", "PlaneacionUnidadId", "PublicId", "RevisorId", "UpdatedAt" },
                values: new object[] { 1L, true, "La evidencia de la Unidad I debe estar mejor relacionada con el resultado de aprendizaje.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, "ABIERTA", 1L, 1L, new Guid("22222222-aaaa-0000-0000-000000000001"), 4L, null });

            migrationBuilder.CreateIndex(
                name: "IX_academia_usuarios_UsuarioId",
                table: "academia_usuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_academias_Nombre",
                table: "academias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_AcademiaId",
                table: "asignaturas",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_asignaturas_Clave",
                table: "asignaturas",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_AcademiaId",
                table: "carga_academica",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_AsignaturaId",
                table: "carga_academica",
                column: "AsignaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_DocenteId",
                table: "carga_academica",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_GrupoId",
                table: "carga_academica",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_PeriodoId_GrupoId_AsignaturaId_DocenteId",
                table: "carga_academica",
                columns: new[] { "PeriodoId", "GrupoId", "AsignaturaId", "DocenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_carga_academica_RevisorId",
                table: "carga_academica",
                column: "RevisorId");

            migrationBuilder.CreateIndex(
                name: "IX_carreras_Clave",
                table: "carreras",
                column: "Clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_chat_mensajes_ChatId",
                table: "chat_mensajes",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_mensajes_CreatedAt",
                table: "chat_mensajes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_chat_mensajes_UsuarioId",
                table: "chat_mensajes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_participantes_UsuarioId",
                table: "chat_participantes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats",
                column: "PlaneacionDidacticaId");

            migrationBuilder.CreateIndex(
                name: "IX_ciclos_escolares_Nombre",
                table: "ciclos_escolares",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_documentos_Estado",
                table: "documentos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_HashSha256",
                table: "documentos",
                column: "HashSha256");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_SubidoPorId",
                table: "documentos",
                column: "SubidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_documentos_TipoDocumento",
                table: "documentos",
                column: "TipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_CarreraId",
                table: "grupos",
                column: "CarreraId");

            migrationBuilder.CreateIndex(
                name: "IX_grupos_PeriodoId_Nombre",
                table: "grupos",
                columns: new[] { "PeriodoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_periodos_CicloEscolarId_Nombre",
                table: "periodos",
                columns: new[] { "CicloEscolarId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_actividades_PlaneacionUnidadId_Orden",
                table: "planeacion_actividades",
                columns: new[] { "PlaneacionUnidadId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_docentes_DocenteId",
                table: "planeacion_docentes",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_grupos_GrupoId",
                table: "planeacion_grupos",
                column: "GrupoId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_observaciones_PlaneacionDidacticaId",
                table: "planeacion_observaciones",
                column: "PlaneacionDidacticaId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_observaciones_PlaneacionUnidadId",
                table: "planeacion_observaciones",
                column: "PlaneacionUnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_observaciones_RevisorId",
                table: "planeacion_observaciones",
                column: "RevisorId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_unidades_PlaneacionDidacticaId_Orden",
                table: "planeacion_unidades",
                columns: new[] { "PlaneacionDidacticaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_unidades_UltimaModificacionPorId",
                table: "planeacion_unidades",
                column: "UltimaModificacionPorId");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_AcademiaId",
                table: "planeaciones_didacticas",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_AsignaturaId",
                table: "planeaciones_didacticas",
                column: "AsignaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_Estado",
                table: "planeaciones_didacticas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_PeriodoId_AsignaturaId",
                table: "planeaciones_didacticas",
                columns: new[] { "PeriodoId", "AsignaturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_ProgramaAsignaturaId",
                table: "planeaciones_didacticas",
                column: "ProgramaAsignaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_RevisorId",
                table: "planeaciones_didacticas",
                column: "RevisorId");

            migrationBuilder.CreateIndex(
                name: "IX_planeaciones_didacticas_UltimaModificacionPorId",
                table: "planeaciones_didacticas",
                column: "UltimaModificacionPorId");

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_AcademiaId",
                table: "programas_asignatura",
                column: "AcademiaId");

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_AsignaturaId",
                table: "programas_asignatura",
                column: "AsignaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_ClaveAsignatura",
                table: "programas_asignatura",
                column: "ClaveAsignatura");

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_DocumentoId",
                table: "programas_asignatura",
                column: "DocumentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_NombreAsignatura",
                table: "programas_asignatura",
                column: "NombreAsignatura");

            migrationBuilder.CreateIndex(
                name: "IX_programas_asignatura_UltimaModificacionPorId",
                table: "programas_asignatura",
                column: "UltimaModificacionPorId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_roles_RolId",
                table: "usuario_roles",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "academia_usuarios");

            migrationBuilder.DropTable(
                name: "carga_academica");

            migrationBuilder.DropTable(
                name: "chat_mensajes");

            migrationBuilder.DropTable(
                name: "chat_participantes");

            migrationBuilder.DropTable(
                name: "planeacion_actividades");

            migrationBuilder.DropTable(
                name: "planeacion_docentes");

            migrationBuilder.DropTable(
                name: "planeacion_grupos");

            migrationBuilder.DropTable(
                name: "planeacion_observaciones");

            migrationBuilder.DropTable(
                name: "usuario_roles");

            migrationBuilder.DropTable(
                name: "chats");

            migrationBuilder.DropTable(
                name: "grupos");

            migrationBuilder.DropTable(
                name: "planeacion_unidades");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "carreras");

            migrationBuilder.DropTable(
                name: "planeaciones_didacticas");

            migrationBuilder.DropTable(
                name: "periodos");

            migrationBuilder.DropTable(
                name: "programas_asignatura");

            migrationBuilder.DropTable(
                name: "ciclos_escolares");

            migrationBuilder.DropTable(
                name: "asignaturas");

            migrationBuilder.DropTable(
                name: "documentos");

            migrationBuilder.DropTable(
                name: "academias");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
