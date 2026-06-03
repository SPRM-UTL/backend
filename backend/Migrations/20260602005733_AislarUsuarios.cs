using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AislarUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sk_usuario_id",
                table: "Dim_Gesto",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sk_usuario_id",
                table: "Dim_Aparato",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Gesto_sk_usuario_id",
                table: "Dim_Gesto",
                column: "sk_usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Aparato_sk_usuario_id",
                table: "Dim_Aparato",
                column: "sk_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dim_Aparato_dim_usuario_sk_usuario_id",
                table: "Dim_Aparato",
                column: "sk_usuario_id",
                principalTable: "dim_usuario",
                principalColumn: "sk_usuario_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dim_Gesto_dim_usuario_sk_usuario_id",
                table: "Dim_Gesto",
                column: "sk_usuario_id",
                principalTable: "dim_usuario",
                principalColumn: "sk_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dim_Aparato_dim_usuario_sk_usuario_id",
                table: "Dim_Aparato");

            migrationBuilder.DropForeignKey(
                name: "FK_Dim_Gesto_dim_usuario_sk_usuario_id",
                table: "Dim_Gesto");

            migrationBuilder.DropIndex(
                name: "IX_Dim_Gesto_sk_usuario_id",
                table: "Dim_Gesto");

            migrationBuilder.DropIndex(
                name: "IX_Dim_Aparato_sk_usuario_id",
                table: "Dim_Aparato");

            migrationBuilder.DropColumn(
                name: "sk_usuario_id",
                table: "Dim_Gesto");

            migrationBuilder.DropColumn(
                name: "sk_usuario_id",
                table: "Dim_Aparato");
        }
    }
}
