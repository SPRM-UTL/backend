using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddCasasHabitaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sk_habitacion_id",
                table: "aparato",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "casa",
                columns: table => new
                {
                    sk_casa_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_casa = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sk_usuario_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_casa", x => x.sk_casa_id);
                    table.ForeignKey(
                        name: "FK_casa_usuario_sk_usuario_id",
                        column: x => x.sk_usuario_id,
                        principalTable: "usuario",
                        principalColumn: "sk_usuario_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "habitacion",
                columns: table => new
                {
                    sk_habitacion_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_habitacion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sk_casa_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_habitacion", x => x.sk_habitacion_id);
                    table.ForeignKey(
                        name: "FK_habitacion_casa_sk_casa_id",
                        column: x => x.sk_casa_id,
                        principalTable: "casa",
                        principalColumn: "sk_casa_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateIndex(
                name: "IX_aparato_sk_habitacion_id",
                table: "aparato",
                column: "sk_habitacion_id");

            migrationBuilder.CreateIndex(
                name: "IX_casa_sk_usuario_id",
                table: "casa",
                column: "sk_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_habitacion_sk_casa_id",
                table: "habitacion",
                column: "sk_casa_id");

            migrationBuilder.AddForeignKey(
                name: "FK_aparato_habitacion_sk_habitacion_id",
                table: "aparato",
                column: "sk_habitacion_id",
                principalTable: "habitacion",
                principalColumn: "sk_habitacion_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_aparato_habitacion_sk_habitacion_id",
                table: "aparato");

            migrationBuilder.DropTable(
                name: "habitacion");

            migrationBuilder.DropTable(
                name: "casa");

            migrationBuilder.DropIndex(
                name: "IX_aparato_sk_habitacion_id",
                table: "aparato");

            migrationBuilder.DropColumn(
                name: "sk_habitacion_id",
                table: "aparato");
        }
    }
}
