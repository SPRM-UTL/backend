using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGestoPasos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gesto_paso",
                columns: table => new
                {
                    sk_gesto_paso_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_gesto_id = table.Column<int>(type: "int", nullable: false),
                    orden = table.Column<int>(type: "int", nullable: false),
                    es_activador = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    nombre_gesto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mano_objetivo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cuadros_requeridos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gesto_paso", x => x.sk_gesto_paso_id);
                    table.ForeignKey(
                        name: "FK_gesto_paso_gesto_sk_gesto_id",
                        column: x => x.sk_gesto_id,
                        principalTable: "gesto",
                        principalColumn: "sk_gesto_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_gesto_paso_sk_gesto_id",
                table: "gesto_paso",
                column: "sk_gesto_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gesto_paso");
        }
    }
}
