using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class Crear_Data_Warehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dim_Aparato",
                columns: table => new
                {
                    sk_aparato_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_aparato = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo_aparato = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accion_nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comando_bluetooth = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Aparato", x => x.sk_aparato_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Dim_Gesto",
                columns: table => new
                {
                    sk_gesto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    bk_gesto_id = table.Column<int>(type: "int", nullable: false),
                    nombre_gesto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    identificador_ia = table.Column<int>(type: "int", nullable: false),
                    nivel_confianza_minimo = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    tipo_disparador_nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Gesto", x => x.sk_gesto_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Dim_Tiempo",
                columns: table => new
                {
                    sk_tiempo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fecha_completa = table.Column<DateOnly>(type: "date", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    mes_numero = table.Column<int>(type: "int", nullable: false),
                    mes_nombre = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_semana_nombre = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hora_periodo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Tiempo", x => x.sk_tiempo_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Dim_Usuario",
                columns: table => new
                {
                    sk_usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    bk_usuario_id = table.Column<int>(type: "int", nullable: false),
                    nombre_usuario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_usuario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_arduino = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mac_address_usuario = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dim_Usuario", x => x.sk_usuario_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "Historico_Actividad",
                columns: table => new
                {
                    sk_actividad_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_usuario_id = table.Column<int>(type: "int", nullable: false),
                    sk_gesto_id = table.Column<int>(type: "int", nullable: false),
                    sk_aparato_id = table.Column<int>(type: "int", nullable: false),
                    sk_tiempo_id = table.Column<int>(type: "int", nullable: false),
                    confianza_ia = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    tiempo_respuesta = table.Column<int>(type: "int", nullable: false),
                    ejecucion_exitosa = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historico_Actividad", x => x.sk_actividad_id);
                    table.ForeignKey(
                        name: "FK_Historico_Actividad_Dim_Aparato_sk_aparato_id",
                        column: x => x.sk_aparato_id,
                        principalTable: "Dim_Aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Actividad_Dim_Gesto_sk_gesto_id",
                        column: x => x.sk_gesto_id,
                        principalTable: "Dim_Gesto",
                        principalColumn: "sk_gesto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Actividad_Dim_Tiempo_sk_tiempo_id",
                        column: x => x.sk_tiempo_id,
                        principalTable: "Dim_Tiempo",
                        principalColumn: "sk_tiempo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Historico_Actividad_Dim_Usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "Dim_Usuario",
                        principalColumn: "sk_usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Actividad_sk_aparato_id",
                table: "Historico_Actividad",
                column: "sk_aparato_id");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Actividad_sk_gesto_id",
                table: "Historico_Actividad",
                column: "sk_gesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Actividad_sk_tiempo_id",
                table: "Historico_Actividad",
                column: "sk_tiempo_id");

            migrationBuilder.CreateIndex(
                name: "IX_Historico_Actividad_sk_usuario_id",
                table: "Historico_Actividad",
                column: "sk_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Historico_Actividad");

            migrationBuilder.DropTable(
                name: "Dim_Aparato");

            migrationBuilder.DropTable(
                name: "Dim_Gesto");

            migrationBuilder.DropTable(
                name: "Dim_Tiempo");

            migrationBuilder.DropTable(
                name: "Dim_Usuario");
        }
    }
}
