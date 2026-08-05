using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class modifyAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpires",
                table: "usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecretKey",
                table: "usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 1L,
                columns: new[] { "AccessFailedCount", "LockoutEnd", "PasswordResetToken", "PasswordResetTokenExpires", "TwoFactorEnabled", "TwoFactorSecretKey" },
                values: new object[] { 0, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 2L,
                columns: new[] { "AccessFailedCount", "LockoutEnd", "PasswordResetToken", "PasswordResetTokenExpires", "TwoFactorEnabled", "TwoFactorSecretKey" },
                values: new object[] { 0, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 3L,
                columns: new[] { "AccessFailedCount", "LockoutEnd", "PasswordResetToken", "PasswordResetTokenExpires", "TwoFactorEnabled", "TwoFactorSecretKey" },
                values: new object[] { 0, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "usuarios",
                keyColumn: "Id",
                keyValue: 4L,
                columns: new[] { "AccessFailedCount", "LockoutEnd", "PasswordResetToken", "PasswordResetTokenExpires", "TwoFactorEnabled", "TwoFactorSecretKey" },
                values: new object[] { 0, null, null, null, false, null });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UsuarioId",
                table: "RefreshToken",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpires",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecretKey",
                table: "usuarios");
        }
    }
}
