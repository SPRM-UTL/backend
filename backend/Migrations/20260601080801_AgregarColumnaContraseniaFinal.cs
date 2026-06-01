using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AgregarColumnaContraseniaFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historico_Actividad_Dim_Usuario_sk_usuario_id",
                table: "Historico_Actividad");

            migrationBuilder.DropForeignKey(
                name: "FK_Token_Dim_Usuario_sk_usuario_id",
                table: "Token");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Token",
                table: "Token");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dim_Usuario",
                table: "Dim_Usuario");

            migrationBuilder.RenameTable(
                name: "Token",
                newName: "token");

            migrationBuilder.RenameTable(
                name: "Dim_Usuario",
                newName: "dim_usuario");

            migrationBuilder.RenameIndex(
                name: "IX_Token_sk_usuario_id",
                table: "token",
                newName: "IX_token_sk_usuario_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_token",
                table: "token",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dim_usuario",
                table: "dim_usuario",
                column: "sk_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Historico_Actividad_dim_usuario_sk_usuario_id",
                table: "Historico_Actividad",
                column: "sk_usuario_id",
                principalTable: "dim_usuario",
                principalColumn: "sk_usuario_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_token_dim_usuario_sk_usuario_id",
                table: "token",
                column: "sk_usuario_id",
                principalTable: "dim_usuario",
                principalColumn: "sk_usuario_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Historico_Actividad_dim_usuario_sk_usuario_id",
                table: "Historico_Actividad");

            migrationBuilder.DropForeignKey(
                name: "FK_token_dim_usuario_sk_usuario_id",
                table: "token");

            migrationBuilder.DropPrimaryKey(
                name: "PK_token",
                table: "token");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dim_usuario",
                table: "dim_usuario");

            migrationBuilder.RenameTable(
                name: "token",
                newName: "Token");

            migrationBuilder.RenameTable(
                name: "dim_usuario",
                newName: "Dim_Usuario");

            migrationBuilder.RenameIndex(
                name: "IX_token_sk_usuario_id",
                table: "Token",
                newName: "IX_Token_sk_usuario_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Token",
                table: "Token",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dim_Usuario",
                table: "Dim_Usuario",
                column: "sk_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Historico_Actividad_Dim_Usuario_sk_usuario_id",
                table: "Historico_Actividad",
                column: "sk_usuario_id",
                principalTable: "Dim_Usuario",
                principalColumn: "sk_usuario_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Token_Dim_Usuario_sk_usuario_id",
                table: "Token",
                column: "sk_usuario_id",
                principalTable: "Dim_Usuario",
                principalColumn: "sk_usuario_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
