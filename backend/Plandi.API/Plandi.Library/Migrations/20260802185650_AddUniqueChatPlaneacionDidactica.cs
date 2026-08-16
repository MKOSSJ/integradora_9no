using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Plandi.Library.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueChatPlaneacionDidactica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats");

            migrationBuilder.CreateIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats",
                column: "PlaneacionDidacticaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats");

            migrationBuilder.CreateIndex(
                name: "IX_chats_PlaneacionDidacticaId",
                table: "chats",
                column: "PlaneacionDidacticaId");
        }
    }
}
