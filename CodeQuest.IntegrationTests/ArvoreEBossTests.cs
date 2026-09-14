using System.Net;
using System.Net.Http.Json;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class ArvoreEBossTests
{
    private readonly CodeQuestApiFactory _factory;

    public ArvoreEBossTests(CodeQuestApiFactory factory) => _factory = factory;

    private static TesteIa TesteIaComQuestoes(int quantidade, int alternativaCorretaIndice = 0)
    {
        var questoes = new List<QuestaoTesteIa>();
        for (var i = 0; i < quantidade; i++)
        {
            var alternativas = new List<AlternativaIa>
            {
                new("A", alternativaCorretaIndice == 0, "ok"),
                new("B", alternativaCorretaIndice == 1, "ok"),
            };
            questoes.Add(new QuestaoTesteIa($"Conceito{i}", $"Enunciado{i}", alternativas));
        }
        return new TesteIa(questoes);
    }

    /// <summary>Cria um módulo próprio (fora do currículo semeado) para não interferir com outros testes.</summary>
    private async Task<Modulo> CriarModuloComTopicoAsync(int nivelTopico, StatusModulo status = StatusModulo.Liberado)
    {
        await using var db = _factory.CriarDbContext();
        var modulo = new Modulo { Nome = $"Módulo Teste {Guid.NewGuid():N}", Status = status };
        db.Modulos.Add(modulo);
        await db.SaveChangesAsync();

        db.Topicos.Add(new Topico { ModuloId = modulo.Id, Nome = "Tópico 1", NivelEstimado = nivelTopico });
        await db.SaveChangesAsync();
        return modulo;
    }

    [Fact]
    public async Task Listar_IncluiCurriculoSemeado()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var modulos = await cliente.ObterJsonAsync<List<ModuloDto>>("/api/arvore");

        Assert.NotNull(modulos);
        Assert.True(modulos!.Count >= 10);
        Assert.Contains(modulos, m => m.Nome == "Fundamentos C#" && m.Ordem == 1);
    }

    [Fact]
    public async Task Listar_SemToken_RetornaUnauthorized()
    {
        var cliente = _factory.CreateClient();

        var resposta = await cliente.GetAsync("/api/arvore");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task IniciarBoss_ModuloBloqueado_RetornaBadRequest()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico, status: StatusModulo.Bloqueado);
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task IniciarBoss_TopicoAbaixoDoNivelMinimo_RetornaBadRequest()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico - 1);
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task IniciarBoss_TopicoNoNivelMinimo_CriaTesteExpertSemRevelarGabarito()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(TesteIaComQuestoes(2));

        var resposta = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);
        resposta.EnsureSuccessStatusCode();
        var teste = await resposta.LerJsonAsync<TesteDto>();

        Assert.NotNull(teste);
        Assert.Equal(modulo.Id, teste!.ModuloId);
        Assert.Equal(Dificuldade.Expert, teste.Dificuldade);
        Assert.Equal(2, teste.Questoes.Count);
        Assert.All(teste.Questoes, q => Assert.Equal(2, q.Alternativas.Count));
    }

    [Fact]
    public async Task ResponderBoss_Aprovado_ConcluiModuloELiberaDependente()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico);

        int dependenteId;
        await using (var db = _factory.CriarDbContext())
        {
            var dependente = new Modulo { Nome = $"Dependente {Guid.NewGuid():N}", Status = StatusModulo.Bloqueado };
            db.Modulos.Add(dependente);
            await db.SaveChangesAsync();
            db.ModuloPrereqs.Add(new ModuloPrereq { ModuloId = dependente.Id, RequerModuloId = modulo.Id });
            await db.SaveChangesAsync();
            dependenteId = dependente.Id;
        }

        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(TesteIaComQuestoes(2, alternativaCorretaIndice: 0));

        var respostaIniciar = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);
        respostaIniciar.EnsureSuccessStatusCode();
        var teste = await respostaIniciar.LerJsonAsync<TesteDto>();

        var respostaCorrigir = await cliente.PostarJsonAsync(
            $"/api/arvore/boss/{teste!.Id}/responder", new ResponderTesteRequest(new List<int> { 0, 0 }));
        respostaCorrigir.EnsureSuccessStatusCode();
        var correcao = await respostaCorrigir.LerJsonAsync<CorrecaoBossDto>();

        Assert.True(correcao!.Aprovado);
        Assert.Equal(100, correcao.Nota);
        Assert.Contains(correcao.ModulosLiberados, nome => nome.StartsWith("Dependente"));

        await using var dbVerificacao = _factory.CriarDbContext();
        var dependenteAtualizado = await dbVerificacao.Modulos.FindAsync(dependenteId);
        Assert.Equal(StatusModulo.Liberado, dependenteAtualizado!.Status);
    }

    [Fact]
    public async Task ResponderBoss_Reprovado_ModuloContinuaLiberado()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(TesteIaComQuestoes(2, alternativaCorretaIndice: 0));

        var respostaIniciar = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);
        var teste = await respostaIniciar.LerJsonAsync<TesteDto>();

        var respostaCorrigir = await cliente.PostarJsonAsync(
            $"/api/arvore/boss/{teste!.Id}/responder", new ResponderTesteRequest(new List<int> { 1, 1 }));
        var correcao = await respostaCorrigir.LerJsonAsync<CorrecaoBossDto>();

        Assert.False(correcao!.Aprovado);
        Assert.Empty(correcao.ModulosLiberados);

        await using var db = _factory.CriarDbContext();
        var moduloAtualizado = await db.Modulos.FindAsync(modulo.Id);
        Assert.Equal(StatusModulo.Liberado, moduloAtualizado!.Status);
    }

    [Fact]
    public async Task ResponderBoss_DuasVezes_SegundaRetornaBadRequest()
    {
        var modulo = await CriarModuloComTopicoAsync(nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(TesteIaComQuestoes(1));

        var respostaIniciar = await cliente.PostAsync($"/api/arvore/modulos/{modulo.Id}/boss", null);
        var teste = await respostaIniciar.LerJsonAsync<TesteDto>();

        var primeira = await cliente.PostarJsonAsync($"/api/arvore/boss/{teste!.Id}/responder", new ResponderTesteRequest(new List<int> { 0 }));
        primeira.EnsureSuccessStatusCode();

        var segunda = await cliente.PostarJsonAsync($"/api/arvore/boss/{teste.Id}/responder", new ResponderTesteRequest(new List<int> { 0 }));

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }
}
