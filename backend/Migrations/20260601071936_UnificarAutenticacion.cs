using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UnificarAutenticacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Token_usuarios_UsuarioId",
                table: "Token");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropColumn(
                name: "bk_usuario_id",
                table: "Dim_Usuario");

            migrationBuilder.RenameColumn(
                name: "UsuarioId",
                table: "Token",
                newName: "sk_usuario_id");

            migrationBuilder.RenameIndex(
                name: "IX_Token_UsuarioId",
                table: "Token",
                newName: "IX_Token_sk_usuario_id");

            migrationBuilder.AlterColumn<string>(
                name: "Cadena",
                table: "Token",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_usuario",
                table: "Dim_Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_arduino",
                table: "Dim_Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "mac_address_usuario",
                table: "Dim_Usuario",
                type: "varchar(17)",
                maxLength: 17,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldMaxLength: 17)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "email_usuario",
                table: "Dim_Usuario",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<string>(
                name: "contrasenia",
                table: "Dim_Usuario",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "mes_nombre",
                table: "Dim_Tiempo",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "dia_semana_nombre",
                table: "Dim_Tiempo",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_gesto",
                table: "Dim_Gesto",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "tipo_aparato",
                table: "Dim_Aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_aparato",
                table: "Dim_Aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "comando_bluetooth",
                table: "Dim_Aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AlterColumn<string>(
                name: "accion_nombre",
                table: "Dim_Aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Token_Dim_Usuario_sk_usuario_id",
                table: "Token",
                column: "sk_usuario_id",
                principalTable: "Dim_Usuario",
                principalColumn: "sk_usuario_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Token_Dim_Usuario_sk_usuario_id",
                table: "Token");

            migrationBuilder.DropColumn(
                name: "contrasenia",
                table: "Dim_Usuario");

            migrationBuilder.RenameColumn(
                name: "sk_usuario_id",
                table: "Token",
                newName: "UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Token_sk_usuario_id",
                table: "Token",
                newName: "IX_Token_UsuarioId");

            migrationBuilder.UpdateData(
                table: "Token",
                keyColumn: "Cadena",
                keyValue: null,
                column: "Cadena",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Cadena",
                table: "Token",
                type: "longtext",
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Usuario",
                keyColumn: "nombre_usuario",
                keyValue: null,
                column: "nombre_usuario",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_usuario",
                table: "Dim_Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Usuario",
                keyColumn: "nombre_arduino",
                keyValue: null,
                column: "nombre_arduino",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_arduino",
                table: "Dim_Usuario",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Usuario",
                keyColumn: "mac_address_usuario",
                keyValue: null,
                column: "mac_address_usuario",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "mac_address_usuario",
                table: "Dim_Usuario",
                type: "varchar(17)",
                maxLength: 17,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(17)",
                oldMaxLength: 17,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Usuario",
                keyColumn: "email_usuario",
                keyValue: null,
                column: "email_usuario",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "email_usuario",
                table: "Dim_Usuario",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddColumn<int>(
                name: "bk_usuario_id",
                table: "Dim_Usuario",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Dim_Tiempo",
                keyColumn: "mes_nombre",
                keyValue: null,
                column: "mes_nombre",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "mes_nombre",
                table: "Dim_Tiempo",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Tiempo",
                keyColumn: "dia_semana_nombre",
                keyValue: null,
                column: "dia_semana_nombre",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "dia_semana_nombre",
                table: "Dim_Tiempo",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Gesto",
                keyColumn: "nombre_gesto",
                keyValue: null,
                column: "nombre_gesto",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_gesto",
                table: "Dim_Gesto",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Aparato",
                keyColumn: "tipo_aparato",
                keyValue: null,
                column: "tipo_aparato",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "tipo_aparato",
                table: "Dim_Aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Aparato",
                keyColumn: "nombre_aparato",
                keyValue: null,
                column: "nombre_aparato",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "nombre_aparato",
                table: "Dim_Aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Aparato",
                keyColumn: "comando_bluetooth",
                keyValue: null,
                column: "comando_bluetooth",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "comando_bluetooth",
                table: "Dim_Aparato",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.UpdateData(
                table: "Dim_Aparato",
                keyColumn: "accion_nombre",
                keyValue: null,
                column: "accion_nombre",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "accion_nombre",
                table: "Dim_Aparato",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                collation: "utf8mb4_unicode_ci",
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Contrasenia = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "longtext", nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true, collation: "utf8mb4_unicode_ci")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4")
                .Annotation("Relational:Collation", "utf8mb4_unicode_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_Token_usuarios_UsuarioId",
                table: "Token",
                column: "UsuarioId",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
