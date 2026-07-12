using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlinharModeloSecao2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Topico_NivelEstimado",
                table: "Topicos",
                sql: "\"NivelEstimado\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Teste_Nota",
                table: "Testes",
                sql: "\"Nota\" IS NULL OR \"Nota\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Tentativa_Nota",
                table: "Tentativas",
                sql: "\"Nota\" BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SessaoFoco_Alvo",
                table: "SessoesFoco",
                sql: "num_nonnulls(\"ProjetoId\", \"TopicoId\") = 1");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Modulo_NotaBoss",
                table: "Modulos",
                sql: "\"NotaBoss\" IS NULL OR \"NotaBoss\" BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Topico_NivelEstimado",
                table: "Topicos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Teste_Nota",
                table: "Testes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Tentativa_Nota",
                table: "Tentativas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SessaoFoco_Alvo",
                table: "SessoesFoco");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Modulo_NotaBoss",
                table: "Modulos");
        }
    }
}
