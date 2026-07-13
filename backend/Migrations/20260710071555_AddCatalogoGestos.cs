using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogoGestos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "catalogo_gesto",
                columns: table => new
                {
                    sk_catalogo_gesto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_body_gesture = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_catalogo_gesto", x => x.sk_catalogo_gesto_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "usuario_gesto_config",
                columns: table => new
                {
                    sk_usuario_id = table.Column<int>(type: "int", nullable: false),
                    sk_catalogo_gesto_id = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_gesto_config", x => new { x.sk_usuario_id, x.sk_catalogo_gesto_id });
                    table.ForeignKey(
                        name: "FK_usuario_gesto_config_catalogo_gesto_sk_catalogo_gesto_id",
                        column: x => x.sk_catalogo_gesto_id,
                        principalTable: "catalogo_gesto",
                        principalColumn: "sk_catalogo_gesto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_gesto_config_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.InsertData(
                table: "catalogo_gesto",
                columns: new[] { "sk_catalogo_gesto_id", "icono", "is_body_gesture", "nombre" },
                values: new object[,]
                {
                    { 1, "lucide_hand", false, "Manos Arriba" },
                    { 2, "lucide_hand", false, "Una Mano Arriba" },
                    { 3, "lucide_hand", false, "Agitar la Mano" },
                    { 4, "lucide_hand", false, "Abrir Puño" },
                    { 5, "lucide_hand", false, "Cerrar Puño" },
                    { 6, "lucide_thumbs_up", false, "A PULGAR ARRIBA" },
                    { 7, "lucide_thumbs_down", false, "A PULGAR ABAJO" },
                    { 8, "lucide_hand", false, "B CUATRO" },
                    { 9, "lucide_hand", false, "D UNO" },
                    { 10, "lucide_check", false, "F OK" },
                    { 11, "lucide_hand", false, "I" },
                    { 12, "lucide_hand", false, "L" },
                    { 13, "lucide_hand", false, "U" },
                    { 14, "lucide_heart", false, "V PAZ" },
                    { 15, "lucide_hand", false, "W TRES" },
                    { 16, "lucide_hand", false, "Y" },
                    { 17, "lucide_hand", false, "PUÑO" },
                    { 18, "lucide_hand", false, "CINCO MANO ABIERTA" },
                    { 19, "lucide_star", false, "ROCK" },
                    { 20, "lucide_heart", false, "TE AMO ILY" },
                    { 21, "lucide_zap", true, "Sentadillas" },
                    { 22, "lucide_check", true, "Decir si con la cabeza" },
                    { 23, "lucide_power", true, "Decir no con la cabeza" },
                    { 24, "lucide_star", true, "Aplaudir" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuario_gesto_config_sk_catalogo_gesto_id",
                table: "usuario_gesto_config",
                column: "sk_catalogo_gesto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_gesto_config");

            migrationBuilder.DropTable(
                name: "catalogo_gesto");
        }
    }
}
