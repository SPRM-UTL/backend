using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "esp32_device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DeviceKey = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_esp32_device", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "tiempo",
                columns: table => new
                {
                    sk_tiempo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fecha_completa = table.Column<DateOnly>(type: "date", nullable: false),
                    anio = table.Column<int>(type: "int", nullable: false),
                    mes_numero = table.Column<int>(type: "int", nullable: false),
                    mes_nombre = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dia_semana_nombre = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hora_periodo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiempo", x => x.sk_tiempo_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    sk_usuario_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_usuario = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_usuario = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrasenia = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_arduino = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mac_address_usuario = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.sk_usuario_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "esp32_message",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SourceDeviceId = table.Column<int>(type: "int", nullable: false),
                    TargetDeviceId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Response = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WasProcessed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ProcessingError = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_esp32_message", x => x.Id);
                    table.ForeignKey(
                        name: "FK_esp32_message_esp32_device_SourceDeviceId",
                        column: x => x.SourceDeviceId,
                        principalTable: "esp32_device",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_esp32_message_esp32_device_TargetDeviceId",
                        column: x => x.TargetDeviceId,
                        principalTable: "esp32_device",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "aparato",
                columns: table => new
                {
                    sk_aparato_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_aparato = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo_aparato = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accion_nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comando_bluetooth = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mac_bluetooth = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_bluetooth = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_sincronizacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    sk_usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato", x => x.sk_aparato_id);
                    table.ForeignKey(
                        name: "FK_aparato_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "token",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cadena = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fecha_expiracion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fecha_baja = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    sk_usuario_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token", x => x.id);
                    table.ForeignKey(
                        name: "FK_token_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "gesto",
                columns: table => new
                {
                    sk_gesto_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    bk_gesto_id = table.Column<int>(type: "int", nullable: false),
                    nombre_gesto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    identificador_ia = table.Column<int>(type: "int", nullable: false),
                    nivel_confianza_minimo = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    tipo_disparador_nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sk_aparato_id = table.Column<int>(type: "int", nullable: true),
                    sk_usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gesto", x => x.sk_gesto_id);
                    table.ForeignKey(
                        name: "FK_gesto_aparato_sk_aparato_id",
                        column: x => x.sk_aparato_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id");
                    table.ForeignKey(
                        name: "FK_gesto_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id");
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "historial_actividad",
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
                    table.PrimaryKey("PK_historial_actividad", x => x.sk_actividad_id);
                    table.ForeignKey(
                        name: "FK_historial_actividad_aparato_sk_aparato_id",
                        column: x => x.sk_aparato_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_actividad_gesto_sk_gesto_id",
                        column: x => x.sk_gesto_id,
                        principalTable: "gesto",
                        principalColumn: "sk_gesto_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_actividad_tiempo_sk_tiempo_id",
                        column: x => x.sk_tiempo_id,
                        principalTable: "tiempo",
                        principalColumn: "sk_tiempo_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_historial_actividad_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_sk_usuario_id",
                table: "aparato",
                column: "sk_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_esp32_message_SourceDeviceId",
                table: "esp32_message",
                column: "SourceDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_esp32_message_TargetDeviceId",
                table: "esp32_message",
                column: "TargetDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_gesto_sk_aparato_id",
                table: "gesto",
                column: "sk_aparato_id");

            migrationBuilder.CreateIndex(
                name: "IX_gesto_sk_usuario_id",
                table: "gesto",
                column: "sk_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_actividad_sk_aparato_id",
                table: "historial_actividad",
                column: "sk_aparato_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_actividad_sk_gesto_id",
                table: "historial_actividad",
                column: "sk_gesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_actividad_sk_tiempo_id",
                table: "historial_actividad",
                column: "sk_tiempo_id");

            migrationBuilder.CreateIndex(
                name: "IX_historial_actividad_sk_usuario_id",
                table: "historial_actividad",
                column: "sk_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_token_sk_usuario_id",
                table: "token",
                column: "sk_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "esp32_message");

            migrationBuilder.DropTable(
                name: "historial_actividad");

            migrationBuilder.DropTable(
                name: "token");

            migrationBuilder.DropTable(
                name: "esp32_device");

            migrationBuilder.DropTable(
                name: "gesto");

            migrationBuilder.DropTable(
                name: "tiempo");

            migrationBuilder.DropTable(
                name: "aparato");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
