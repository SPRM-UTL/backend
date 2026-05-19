using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "edad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "fecha",
                table: "usuarios");

            migrationBuilder.AddColumn<string>(
                name: "Contrasenia",
                table: "usuarios",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "usuarios",
                type: "longtext",
                nullable: true,
                collation: "utf8mb4_unicode_ci")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Contrasenia",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "usuarios");

            migrationBuilder.AddColumn<int>(
                name: "edad",
                table: "usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha",
                table: "usuarios",
                type: "datetime",
                nullable: true);
        }
    }
}
