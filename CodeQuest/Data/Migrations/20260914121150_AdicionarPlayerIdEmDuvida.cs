using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPlayerIdEmDuvida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dúvidas anteriores a esta migration nunca tiveram dono (bug corrigido aqui) — não
            // há como atribuí-las a um player de forma confiável, então são removidas antes de
            // PlayerId virar obrigatório (decisão do usuário: dev, não produção).
            migrationBuilder.Sql("DELETE FROM \"Duvidas\";");

            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "Duvidas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Duvidas_PlayerId",
                table: "Duvidas",
                column: "PlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Duvidas_Players_PlayerId",
                table: "Duvidas",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duvidas_Players_PlayerId",
                table: "Duvidas");

            migrationBuilder.DropIndex(
                name: "IX_Duvidas_PlayerId",
                table: "Duvidas");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Duvidas");
        }
    }
}
