using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CodeQuest.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItensLoja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CustoGold = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensLoja", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    NotaBoss = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    XpTotal = table.Column<int>(type: "integer", nullable: false),
                    Nivel = table.Column<int>(type: "integer", nullable: false),
                    Gold = table.Column<int>(type: "integer", nullable: false),
                    StreakDias = table.Column<int>(type: "integer", nullable: false),
                    UltimoDiaAtivo = table.Column<DateOnly>(type: "date", nullable: true),
                    PocaoStreakAtiva = table.Column<bool>(type: "boolean", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.CheckConstraint("CK_Player_Gold", "\"Gold\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuloPrereqs",
                columns: table => new
                {
                    ModuloId = table.Column<int>(type: "integer", nullable: false),
                    RequerModuloId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuloPrereqs", x => new { x.ModuloId, x.RequerModuloId });
                    table.ForeignKey(
                        name: "FK_ModuloPrereqs_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModuloPrereqs_Modulos_RequerModuloId",
                        column: x => x.RequerModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Topicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModuloId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    NivelEstimado = table.Column<int>(type: "integer", nullable: false),
                    Prioridade = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topicos_Modulos_ModuloId",
                        column: x => x.ModuloId,
                        principalTable: "Modulos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComprasLoja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemLojaId = table.Column<int>(type: "integer", nullable: false),
                    CustoGoldPago = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComprasLoja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComprasLoja_ItensLoja_ItemLojaId",
                        column: x => x.ItemLojaId,
                        principalTable: "ItensLoja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComprasLoja_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Duvidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicoId = table.Column<int>(type: "integer", nullable: true),
                    Pergunta = table.Column<string>(type: "text", nullable: false),
                    RespostaJson = table.Column<string>(type: "text", nullable: true),
                    MarcadaParaRevisao = table.Column<bool>(type: "boolean", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duvidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Duvidas_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Exercicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicoId = table.Column<int>(type: "integer", nullable: false),
                    Dificuldade = table.Column<string>(type: "text", nullable: false),
                    Formato = table.Column<string>(type: "text", nullable: false),
                    EnunciadoJson = table.Column<string>(type: "text", nullable: false),
                    GabaritoJson = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercicios_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Missoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    TopicoId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    XpRecompensa = table.Column<int>(type: "integer", nullable: false),
                    GoldRecompensa = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataAlvo = table.Column<DateOnly>(type: "date", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrigemJson = table.Column<string>(type: "text", nullable: true),
                    PlayerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missoes_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Missoes_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessoesFoco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    TopicoId = table.Column<int>(type: "integer", nullable: true),
                    MinutosPlanejados = table.Column<int>(type: "integer", nullable: false),
                    MinutosReais = table.Column<int>(type: "integer", nullable: false),
                    XpGanho = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesFoco", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesFoco_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessoesFoco_Projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "Projetos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessoesFoco_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Testes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TopicoId = table.Column<int>(type: "integer", nullable: false),
                    Dificuldade = table.Column<string>(type: "text", nullable: false),
                    QuestoesJson = table.Column<string>(type: "text", nullable: false),
                    RespostasJson = table.Column<string>(type: "text", nullable: true),
                    Nota = table.Column<int>(type: "integer", nullable: true),
                    GapsJson = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Testes_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tentativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExercicioId = table.Column<int>(type: "integer", nullable: false),
                    Resposta = table.Column<string>(type: "text", nullable: false),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    FeedbackJson = table.Column<string>(type: "text", nullable: true),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tentativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tentativas_Exercicios_ExercicioId",
                        column: x => x.ExercicioId,
                        principalTable: "Exercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MissaoExercicios",
                columns: table => new
                {
                    MissaoId = table.Column<int>(type: "integer", nullable: false),
                    ExercicioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissaoExercicios", x => new { x.MissaoId, x.ExercicioId });
                    table.ForeignKey(
                        name: "FK_MissaoExercicios_Exercicios_ExercicioId",
                        column: x => x.ExercicioId,
                        principalTable: "Exercicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissaoExercicios_Missoes_MissaoId",
                        column: x => x.MissaoId,
                        principalTable: "Missoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Revisoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TentativaId = table.Column<int>(type: "integer", nullable: false),
                    AgendadaPara = table.Column<DateOnly>(type: "date", nullable: false),
                    Ciclo = table.Column<int>(type: "integer", nullable: false),
                    Concluida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revisoes_Tentativas_TentativaId",
                        column: x => x.TentativaId,
                        principalTable: "Tentativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComprasLoja_ItemLojaId",
                table: "ComprasLoja",
                column: "ItemLojaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComprasLoja_PlayerId",
                table: "ComprasLoja",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Duvidas_TopicoId",
                table: "Duvidas",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercicios_TopicoId",
                table: "Exercicios",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_MissaoExercicios_ExercicioId",
                table: "MissaoExercicios",
                column: "ExercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_PlayerId",
                table: "Missoes",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Missoes_TopicoId",
                table: "Missoes",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuloPrereqs_RequerModuloId",
                table: "ModuloPrereqs",
                column: "RequerModuloId");

            migrationBuilder.CreateIndex(
                name: "IX_Revisoes_TentativaId",
                table: "Revisoes",
                column: "TentativaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessoesFoco_PlayerId",
                table: "SessoesFoco",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesFoco_ProjetoId",
                table: "SessoesFoco",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesFoco_TopicoId",
                table: "SessoesFoco",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tentativas_ExercicioId",
                table: "Tentativas",
                column: "ExercicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Testes_TopicoId",
                table: "Testes",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Topicos_ModuloId",
                table: "Topicos",
                column: "ModuloId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComprasLoja");

            migrationBuilder.DropTable(
                name: "Duvidas");

            migrationBuilder.DropTable(
                name: "MissaoExercicios");

            migrationBuilder.DropTable(
                name: "ModuloPrereqs");

            migrationBuilder.DropTable(
                name: "Revisoes");

            migrationBuilder.DropTable(
                name: "SessoesFoco");

            migrationBuilder.DropTable(
                name: "Testes");

            migrationBuilder.DropTable(
                name: "ItensLoja");

            migrationBuilder.DropTable(
                name: "Missoes");

            migrationBuilder.DropTable(
                name: "Tentativas");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Exercicios");

            migrationBuilder.DropTable(
                name: "Topicos");

            migrationBuilder.DropTable(
                name: "Modulos");
        }
    }
}
