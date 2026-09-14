using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeQuest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarBossFightTeste : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Testes_Topicos_TopicoId",
                table: "Testes");

            migrationBuilder.AlterColumn<int>(
                name: "TopicoId",
                table: "Testes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ModuloId",
                table: "Testes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Testes_ModuloId",
                table: "Testes",
                column: "ModuloId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Teste_Alvo",
                table: "Testes",
                sql: "num_nonnulls(\"ModuloId\", \"TopicoId\") = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Testes_Modulos_ModuloId",
                table: "Testes",
                column: "ModuloId",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Testes_Topicos_TopicoId",
                table: "Testes",
                column: "TopicoId",
                principalTable: "Topicos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Testes_Modulos_ModuloId",
                table: "Testes");

            migrationBuilder.DropForeignKey(
                name: "FK_Testes_Topicos_TopicoId",
                table: "Testes");

            migrationBuilder.DropIndex(
                name: "IX_Testes_ModuloId",
                table: "Testes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Teste_Alvo",
                table: "Testes");

            migrationBuilder.DropColumn(
                name: "ModuloId",
                table: "Testes");

            migrationBuilder.AlterColumn<int>(
                name: "TopicoId",
                table: "Testes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Testes_Topicos_TopicoId",
                table: "Testes",
                column: "TopicoId",
                principalTable: "Topicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
