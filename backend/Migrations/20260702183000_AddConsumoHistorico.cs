using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    public partial class AddConsumoHistorico : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "corriente_actual",
                table: "aparato_configuracion_red",
                type: "decimal(8,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "potencia_actual",
                table: "aparato_configuracion_red",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "energia_acumulada_wh",
                table: "aparato_configuracion_red",
                type: "decimal(12,3)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_medicion_consumo",
                table: "aparato_configuracion_red",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aparato_consumo_historico",
                columns: table => new
                {
                    sk_consumo_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_aparato_configuracion_red_id = table.Column<int>(type: "int", nullable: false),
                    corriente_a = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    potencia_w = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    energia_wh = table.Column<decimal>(type: "decimal(12,3)", nullable: false),
                    fecha_medicion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_consumo_historico", x => x.sk_consumo_id);
                    table.ForeignKey(
                        name: "FK_aparato_consumo_historico_aparato_configuracion_red_sk_apar~",
                        column: x => x.sk_aparato_configuracion_red_id,
                        principalTable: "aparato_configuracion_red",
                        principalColumn: "sk_aparato_configuracion_red_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_consumo_historico_sk_aparato_configuracion_red_id",
                table: "aparato_consumo_historico",
                column: "sk_aparato_configuracion_red_id");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_consumo_historico_fecha_medicion",
                table: "aparato_consumo_historico",
                column: "fecha_medicion");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aparato_consumo_historico");

            migrationBuilder.DropColumn(
                name: "corriente_actual",
                table: "aparato_configuracion_red");

            migrationBuilder.DropColumn(
                name: "potencia_actual",
                table: "aparato_configuracion_red");

            migrationBuilder.DropColumn(
                name: "energia_acumulada_wh",
                table: "aparato_configuracion_red");

            migrationBuilder.DropColumn(
                name: "fecha_medicion_consumo",
                table: "aparato_configuracion_red");
        }
    }
}
