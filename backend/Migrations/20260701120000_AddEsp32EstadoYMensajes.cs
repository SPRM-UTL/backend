using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddEsp32EstadoYMensajes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "estado_encendido",
                table: "aparato_configuracion_red",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_estado_actualizado",
                table: "aparato_configuracion_red",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen_estado",
                table: "aparato_configuracion_red",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "aparato_mensaje",
                columns: table => new
                {
                    sk_mensaje_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_aparato_configuracion_red_id = table.Column<int>(type: "int", nullable: false),
                    direccion = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payload_json = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comando = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    procesado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    error_procesamiento = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_mensaje", x => x.sk_mensaje_id);
                    table.ForeignKey(
                        name: "FK_aparato_mensaje_aparato_configuracion_red_sk_aparato_configu~",
                        column: x => x.sk_aparato_configuracion_red_id,
                        principalTable: "aparato_configuracion_red",
                        principalColumn: "sk_aparato_configuracion_red_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_mensaje_sk_aparato_configuracion_red_id",
                table: "aparato_mensaje",
                column: "sk_aparato_configuracion_red_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aparato_mensaje");

            migrationBuilder.DropColumn(
                name: "estado_encendido",
                table: "aparato_configuracion_red");

            migrationBuilder.DropColumn(
                name: "fecha_estado_actualizado",
                table: "aparato_configuracion_red");

            migrationBuilder.DropColumn(
                name: "origen_estado",
                table: "aparato_configuracion_red");
        }
    }
}
