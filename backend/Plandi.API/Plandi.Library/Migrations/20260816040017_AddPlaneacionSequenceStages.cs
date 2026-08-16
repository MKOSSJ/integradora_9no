using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaneacionSequenceStages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_planeacion_secuencias_PlaneacionUnidadId_Orden",
                table: "planeacion_secuencias");

            migrationBuilder.AddColumn<int>(
                name: "MetodoTecnica",
                table: "planeacion_secuencias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PlaneacionEtapaSecuenciaId",
                table: "planeacion_secuencias",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "planeacion_etapas_secuencia",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionUnidadId = table.Column<long>(type: "bigint", nullable: false),
                    Fase = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_etapas_secuencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeacion_etapas_secuencia_planeacion_unidades_PlaneacionUnidadId",
                        column: x => x.PlaneacionUnidadId,
                        principalTable: "planeacion_unidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planeacion_secuencia_recursos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlaneacionSecuenciaId = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planeacion_secuencia_recursos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planeacion_secuencia_recursos_planeacion_secuencias_PlaneacionSecuenciaId",
                        column: x => x.PlaneacionSecuenciaId,
                        principalTable: "planeacion_secuencias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Conserva las planeaciones existentes: toda unidad recibe las tres
            // etapas, incluso si no tenía actividades, y cada fila heredada se
            // enlaza con la etapa que corresponde a su Fase.
            migrationBuilder.Sql("""
                INSERT INTO [planeacion_etapas_secuencia]
                    ([PlaneacionUnidadId], [Fase], [PublicId], [Activo], [CreatedAt], [UpdatedAt], [DeletedAt])
                SELECT u.[Id], f.[Fase], NEWID(), u.[Activo], u.[CreatedAt], u.[UpdatedAt], u.[DeletedAt]
                FROM [planeacion_unidades] AS u
                CROSS JOIN (VALUES (1), (2), (3)) AS f([Fase])
                WHERE NOT EXISTS (
                    SELECT 1 FROM [planeacion_etapas_secuencia] AS e
                    WHERE e.[PlaneacionUnidadId] = u.[Id] AND e.[Fase] = f.[Fase]);
                """);

            migrationBuilder.Sql("""
                UPDATE s
                SET [PlaneacionEtapaSecuenciaId] = e.[Id]
                FROM [planeacion_secuencias] AS s
                INNER JOIN [planeacion_etapas_secuencia] AS e
                    ON e.[PlaneacionUnidadId] = s.[PlaneacionUnidadId]
                    AND e.[Fase] = s.[Fase];
                """);

            migrationBuilder.AlterColumn<long>(
                name: "PlaneacionEtapaSecuenciaId",
                table: "planeacion_secuencias",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_secuencias_PlaneacionEtapaSecuenciaId_Orden",
                table: "planeacion_secuencias",
                columns: new[] { "PlaneacionEtapaSecuenciaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_secuencias_PlaneacionUnidadId",
                table: "planeacion_secuencias",
                column: "PlaneacionUnidadId");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_etapas_secuencia_PlaneacionUnidadId_Fase",
                table: "planeacion_etapas_secuencia",
                columns: new[] { "PlaneacionUnidadId", "Fase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_secuencia_recursos_PlaneacionSecuenciaId_Orden",
                table: "planeacion_secuencia_recursos",
                columns: new[] { "PlaneacionSecuenciaId", "Orden" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_planeacion_secuencias_planeacion_etapas_secuencia_PlaneacionEtapaSecuenciaId",
                table: "planeacion_secuencias",
                column: "PlaneacionEtapaSecuenciaId",
                principalTable: "planeacion_etapas_secuencia",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_planeacion_secuencias_planeacion_etapas_secuencia_PlaneacionEtapaSecuenciaId",
                table: "planeacion_secuencias");

            migrationBuilder.DropTable(
                name: "planeacion_etapas_secuencia");

            migrationBuilder.DropTable(
                name: "planeacion_secuencia_recursos");

            migrationBuilder.DropIndex(
                name: "IX_planeacion_secuencias_PlaneacionEtapaSecuenciaId_Orden",
                table: "planeacion_secuencias");

            migrationBuilder.DropIndex(
                name: "IX_planeacion_secuencias_PlaneacionUnidadId",
                table: "planeacion_secuencias");

            migrationBuilder.DropColumn(
                name: "MetodoTecnica",
                table: "planeacion_secuencias");

            migrationBuilder.DropColumn(
                name: "PlaneacionEtapaSecuenciaId",
                table: "planeacion_secuencias");

            migrationBuilder.CreateIndex(
                name: "IX_planeacion_secuencias_PlaneacionUnidadId_Orden",
                table: "planeacion_secuencias",
                columns: new[] { "PlaneacionUnidadId", "Orden" });
        }
    }
}
