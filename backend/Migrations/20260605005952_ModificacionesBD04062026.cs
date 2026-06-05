using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ModificacionesBD04062026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "esp32_message");

            migrationBuilder.DropTable(
                name: "esp32_device");

            migrationBuilder.DropColumn(
                name: "accion_nombre",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "comando_bluetooth",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "mac_bluetooth",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "nombre_bluetooth",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "tipo_aparato",
                table: "aparato");

            migrationBuilder.AddColumn<int>(
                name: "sk_aparato_accion_id",
                table: "aparato",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sk_aparato_tipo_id",
                table: "aparato",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aparato_accion",
                columns: table => new
                {
                    sk_aparato_accion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accion_nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comando_bluetooth = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_accion", x => x.sk_aparato_accion_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "aparato_bluetooth",
                columns: table => new
                {
                    sk_aparato_bluetooth_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_aparato_id = table.Column<int>(type: "int", nullable: false),
                    mac_bluetooth = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre_bluetooth = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_bluetooth", x => x.sk_aparato_bluetooth_id);
                    table.ForeignKey(
                        name: "FK_aparato_bluetooth_aparato_sk_aparato_id",
                        column: x => x.sk_aparato_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "aparato_configuracion_red",
                columns: table => new
                {
                    sk_aparato_configuracion_red_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_aparato_id = table.Column<int>(type: "int", nullable: false),
                    device_key = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mac_address = table.Column<string>(type: "varchar(17)", maxLength: 17, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    host_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    puerto_socket = table.Column<int>(type: "int", nullable: true),
                    protocolo_socket = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ruta_socket = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fecha_ultima_conexion = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_configuracion_red", x => x.sk_aparato_configuracion_red_id);
                    table.ForeignKey(
                        name: "FK_aparato_configuracion_red_aparato_sk_aparato_id",
                        column: x => x.sk_aparato_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "aparato_control",
                columns: table => new
                {
                    sk_aparato_control_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sk_aparato_controlador_id = table.Column<int>(type: "int", nullable: false),
                    sk_aparato_controlado_id = table.Column<int>(type: "int", nullable: false),
                    comando_socket = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_control", x => x.sk_aparato_control_id);
                    table.ForeignKey(
                        name: "FK_aparato_control_aparato_sk_aparato_controlado_id",
                        column: x => x.sk_aparato_controlado_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_aparato_control_aparato_sk_aparato_controlador_id",
                        column: x => x.sk_aparato_controlador_id,
                        principalTable: "aparato",
                        principalColumn: "sk_aparato_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "aparato_tipo",
                columns: table => new
                {
                    sk_aparato_tipo_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_tipo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aparato_tipo", x => x.sk_aparato_tipo_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_sk_aparato_accion_id",
                table: "aparato",
                column: "sk_aparato_accion_id");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_sk_aparato_tipo_id",
                table: "aparato",
                column: "sk_aparato_tipo_id");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_accion_accion_nombre_comando_bluetooth",
                table: "aparato_accion",
                columns: new[] { "accion_nombre", "comando_bluetooth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aparato_bluetooth_sk_aparato_id",
                table: "aparato_bluetooth",
                column: "sk_aparato_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aparato_configuracion_red_device_key",
                table: "aparato_configuracion_red",
                column: "device_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aparato_configuracion_red_sk_aparato_id",
                table: "aparato_configuracion_red",
                column: "sk_aparato_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aparato_control_sk_aparato_controlado_id",
                table: "aparato_control",
                column: "sk_aparato_controlado_id");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_control_sk_aparato_controlador_id_sk_aparato_control~",
                table: "aparato_control",
                columns: new[] { "sk_aparato_controlador_id", "sk_aparato_controlado_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aparato_tipo_nombre_tipo",
                table: "aparato_tipo",
                column: "nombre_tipo",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_aparato_aparato_accion_sk_aparato_accion_id",
                table: "aparato",
                column: "sk_aparato_accion_id",
                principalTable: "aparato_accion",
                principalColumn: "sk_aparato_accion_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_aparato_aparato_tipo_sk_aparato_tipo_id",
                table: "aparato",
                column: "sk_aparato_tipo_id",
                principalTable: "aparato_tipo",
                principalColumn: "sk_aparato_tipo_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aparato_aparato_accion_sk_aparato_accion_id",
                table: "aparato");

            migrationBuilder.DropForeignKey(
                name: "FK_aparato_aparato_tipo_sk_aparato_tipo_id",
                table: "aparato");

            migrationBuilder.DropTable(
                name: "aparato_accion");

            migrationBuilder.DropTable(
                name: "aparato_bluetooth");

            migrationBuilder.DropTable(
                name: "aparato_configuracion_red");

            migrationBuilder.DropTable(
                name: "aparato_control");

            migrationBuilder.DropTable(
                name: "aparato_tipo");

            migrationBuilder.DropIndex(
                name: "IX_aparato_sk_aparato_accion_id",
                table: "aparato");

            migrationBuilder.DropIndex(
                name: "IX_aparato_sk_aparato_tipo_id",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "sk_aparato_accion_id",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "sk_aparato_tipo_id",
                table: "aparato");

            migrationBuilder.AddColumn<string>(
                name: "accion_nombre",
                table: "aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "comando_bluetooth",
                table: "aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "mac_bluetooth",
                table: "aparato",
                type: "varchar(17)",
                maxLength: 17,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "nombre_bluetooth",
                table: "aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "tipo_aparato",
                table: "aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "esp32_device",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeviceKey = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastSeenAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_esp32_device", x => x.Id);
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
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: false, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProcessingError = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Response = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WasProcessed = table.Column<bool>(type: "tinyint(1)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_esp32_message_SourceDeviceId",
                table: "esp32_message",
                column: "SourceDeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_esp32_message_TargetDeviceId",
                table: "esp32_message",
                column: "TargetDeviceId");
        }
    }
}
