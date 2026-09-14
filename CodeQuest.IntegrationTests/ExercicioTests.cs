using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class ExercicioTests
{
    private readonly CodeQuestApiFactory _factory;

    public ExercicioTests(CodeQuestApiFactory factory) => _factory = factory;

    private static ExercicioIa ExercicioIaBasico() => new(
        Titulo: "Some dois números",
        Enunciado: "Escreva uma função que soma dois inteiros.",
        CodigoPartida: "int Somar(int a, int b) => 0;",
        CodigoApresentado: null,
        ComportamentoEsperado: null,
        Alternativas: null,
        ProblemasPlantados: null,
        Solucao: "int Somar(int a, int b) => a + b;",
        CasosDeTeste: null,
        ConceitosAvaliados: ["Aritmética básica"],
        Dicas: ["Use o operador +"]);

    [Fact]
    public async Task Gerar_TopicoDeModuloBloqueado_RetornaBadRequest()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync(status: StatusModulo.Bloqueado);
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/exercicios/gerar",
            new GerarExercicioRequest(topico.Id, Dificuldade.Easy, FormatoExercicio.Codigo));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Gerar_TopicoValido_CriaExercicioSemExporGabarito()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(ExercicioIaBasico());

        var resposta = await cliente.PostarJsonAsync("/api/exercicios/gerar",
            new GerarExercicioRequest(topico.Id, Dificuldade.Easy, FormatoExercicio.Codigo));
        resposta.EnsureSuccessStatusCode();
        var exercicio = await resposta.LerJsonAsync<ExercicioDto>();

        Assert.NotNull(exercicio);
        Assert.Equal(topico.Id, exercicio!.TopicoId);
        Assert.DoesNotContain("Somar(int a, int b) => a + b", exercicio.EnunciadoJson);
    }

    [Fact]
    public async Task Responder_Aprovado_CreditaXpDaTabela()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(ExercicioIaBasico());

        var exercicio = await (await cliente.PostarJsonAsync("/api/exercicios/gerar",
            new GerarExercicioRequest(topico.Id, Dificuldade.Easy, FormatoExercicio.Codigo))).LerJsonAsync<ExercicioDto>();

        _factory.Gemini.Enfileirar(new CorrecaoIa(90, true, null, "Muito bem!", []));
        var respostaCorrecao = await cliente.PostarJsonAsync(
            $"/api/exercicios/{exercicio!.Id}/responder", new ResponderExercicioRequest("int Somar(int a, int b) => a + b;"));
        respostaCorrecao.EnsureSuccessStatusCode();
        var correcao = await respostaCorrecao.LerJsonAsync<CorrecaoDto>();

        Assert.True(correcao!.Aprovado);
        Assert.NotNull(correcao.Recompensa);
        Assert.Equal(15, correcao.Recompensa!.XpBase); // easy = 15 XP base (RN09), antes do multiplicador de streak
        Assert.Null(correcao.RevisaoAgendadaPara);
    }

    [Fact]
    public async Task Responder_Reprovado_AgendaRevisaoENaoCreditaXp()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(ExercicioIaBasico());

        var exercicio = await (await cliente.PostarJsonAsync("/api/exercicios/gerar",
            new GerarExercicioRequest(topico.Id, Dificuldade.Easy, FormatoExercicio.Codigo))).LerJsonAsync<ExercicioDto>();

        _factory.Gemini.Enfileirar(new CorrecaoIa(40, false, null, null, ["Aritmética básica"]));
        var respostaCorrecao = await cliente.PostarJsonAsync(
            $"/api/exercicios/{exercicio!.Id}/responder", new ResponderExercicioRequest("return 0;"));
        respostaCorrecao.EnsureSuccessStatusCode();
        var correcao = await respostaCorrecao.LerJsonAsync<CorrecaoDto>();

        Assert.False(correcao!.Aprovado);
        Assert.Null(correcao.Recompensa);
        Assert.NotNull(correcao.RevisaoAgendadaPara);
    }

    [Fact]
    public async Task Responder_RespostaVazia_RetornaBadRequest()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(ExercicioIaBasico());

        var exercicio = await (await cliente.PostarJsonAsync("/api/exercicios/gerar",
            new GerarExercicioRequest(topico.Id, Dificuldade.Easy, FormatoExercicio.Codigo))).LerJsonAsync<ExercicioDto>();

        var resposta = await cliente.PostarJsonAsync(
            $"/api/exercicios/{exercicio!.Id}/responder", new ResponderExercicioRequest(""));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarTopicos_SoIncluiTopicosDeModulosNaoBloqueados()
    {
        var (_, topicoLiberado) = await _factory.CriarModuloComTopicoAsync(status: StatusModulo.Liberado);
        var (_, topicoBloqueado) = await _factory.CriarModuloComTopicoAsync(status: StatusModulo.Bloqueado);
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var topicos = await cliente.ObterJsonAsync<List<TopicoDto>>("/api/exercicios/topicos");

        Assert.Contains(topicos!, t => t.Id == topicoLiberado.Id);
        Assert.DoesNotContain(topicos!, t => t.Id == topicoBloqueado.Id);
    }
}
