using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionDeModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_aparato_configuracion_red_device_key",
                table: "aparato_configuracion_red");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_configuracion_red_device_key",
                table: "aparato_configuracion_red",
                column: "device_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_aparato_configuracion_red_device_key",
                table: "aparato_configuracion_red");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_configuracion_red_device_key",
                table: "aparato_configuracion_red",
                column: "device_key",
                unique: true);
        }
    }
}
