using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarControlPorVoz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "confirmacion_hablada_activada",
                table: "usuario",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "control_voz_activado",
                table: "usuario",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "voz_idioma",
                table: "usuario",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "voz_tipo_seleccionado",
                table: "usuario",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "voz_velocidad",
                table: "usuario",
                type: "decimal(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "frase_voz_activadora",
                table: "gesto",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confirmacion_hablada_activada",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "control_voz_activado",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "voz_idioma",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "voz_tipo_seleccionado",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "voz_velocidad",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "frase_voz_activadora",
                table: "gesto");
        }
    }
}
