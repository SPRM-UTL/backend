using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGestoAparatoRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "sk_aparato_id",
                table: "Dim_Gesto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dim_Gesto_sk_aparato_id",
                table: "Dim_Gesto",
                column: "sk_aparato_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dim_Gesto_Dim_Aparato_sk_aparato_id",
                table: "Dim_Gesto",
                column: "sk_aparato_id",
                principalTable: "Dim_Aparato",
                principalColumn: "sk_aparato_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dim_Gesto_Dim_Aparato_sk_aparato_id",
                table: "Dim_Gesto");

            migrationBuilder.DropIndex(
                name: "IX_Dim_Gesto_sk_aparato_id",
                table: "Dim_Gesto");

            migrationBuilder.DropColumn(
                name: "sk_aparato_id",
                table: "Dim_Gesto");
        }
    }
}
