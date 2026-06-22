using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGestoDetalleAndMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gesto_detalle",
                columns: table => new
                {
                    sk_gesto_detalle_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_gesto_id = table.Column<int>(type: "int", nullable: false),
                    duracion_segundos = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    iluminacion_recomendada = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    distancia_recomendada = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gesto_detalle", x => x.sk_gesto_detalle_id);
                    table.ForeignKey(
                        name: "FK_gesto_detalle_gesto_sk_gesto_id",
                        column: x => x.sk_gesto_id,
                        principalTable: "gesto",
                        principalColumn: "sk_gesto_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "gesto_media",
                columns: table => new
                {
                    sk_media_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_gesto_detalle_id = table.Column<int>(type: "int", nullable: false),
                    url_archivo = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo_media = table.Column<int>(type: "int", nullable: false),
                    extension = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gesto_media", x => x.sk_media_id);
                    table.ForeignKey(
                        name: "FK_gesto_media_gesto_detalle_sk_gesto_detalle_id",
                        column: x => x.sk_gesto_detalle_id,
                        principalTable: "gesto_detalle",
                        principalColumn: "sk_gesto_detalle_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_gesto_detalle_sk_gesto_id",
                table: "gesto_detalle",
                column: "sk_gesto_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gesto_media_sk_gesto_detalle_id",
                table: "gesto_media",
                column: "sk_gesto_detalle_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gesto_media");

            migrationBuilder.DropTable(
                name: "gesto_detalle");
        }
    }
}
