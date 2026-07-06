using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CodeQuest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasLoja_Players_PlayerId",
                table: "ComprasLoja");

            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Players_PlayerId",
                table: "Missoes");

            migrationBuilder.DropForeignKey(
                name: "FK_SessoesFoco_Players_PlayerId",
                table: "SessoesFoco");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "SessoesFoco",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "Missoes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "ComprasLoja",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Login = table.Column<string>(type: "text", nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_UsuarioId",
                table: "Players",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasLoja_Players_PlayerId",
                table: "ComprasLoja",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Missoes_Players_PlayerId",
                table: "Missoes",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Usuarios_UsuarioId",
                table: "Players",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessoesFoco_Players_PlayerId",
                table: "SessoesFoco",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComprasLoja_Players_PlayerId",
                table: "ComprasLoja");

            migrationBuilder.DropForeignKey(
                name: "FK_Missoes_Players_PlayerId",
                table: "Missoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Players_Usuarios_UsuarioId",
                table: "Players");

            migrationBuilder.DropForeignKey(
                name: "FK_SessoesFoco_Players_PlayerId",
                table: "SessoesFoco");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Players_UsuarioId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Players");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "SessoesFoco",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "Missoes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "PlayerId",
                table: "ComprasLoja",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ComprasLoja_Players_PlayerId",
                table: "ComprasLoja",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Missoes_Players_PlayerId",
                table: "Missoes",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SessoesFoco_Players_PlayerId",
                table: "SessoesFoco",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }
    }
}
