using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Services.Ai;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class MentorTests
{
    private readonly CodeQuestApiFactory _factory;

    public MentorTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Perguntar_SemTopico_DevolveRespostaDoMentor()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(new RespostaMentorIa("Use `var` quando o tipo é óbvio pelo contexto."));

        var resposta = await cliente.PostarJsonAsync("/api/duvidas",
            new PerguntarRequest(null, "Quando devo usar var em C#?"));
        resposta.EnsureSuccessStatusCode();
        var duvida = await resposta.LerJsonAsync<DuvidaDto>();

        Assert.NotNull(duvida);
        Assert.Equal("Quando devo usar var em C#?", duvida!.Pergunta);
        Assert.Contains("var", duvida.Resposta);
        Assert.False(duvida.MarcadaParaRevisao);
    }

    [Fact]
    public async Task Listar_SoIncluiDuvidasDoProprioUsuario()
    {
        var (clienteA, _) = await _factory.RegistrarELogarAsync();
        var (clienteB, _) = await _factory.RegistrarELogarAsync();

        _factory.Gemini.Enfileirar(new RespostaMentorIa("Resposta para A."));
        await clienteA.PostarJsonAsync("/api/duvidas", new PerguntarRequest(null, "Pergunta só da A"));

        var duvidasB = await clienteB.ObterJsonAsync<List<DuvidaDto>>("/api/duvidas");

        Assert.DoesNotContain(duvidasB!, d => d.Pergunta == "Pergunta só da A");
    }

    [Fact]
    public async Task MarcarRevisao_AlternaOFlag()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        _factory.Gemini.Enfileirar(new RespostaMentorIa("Resposta qualquer."));
        var duvida = await (await cliente.PostarJsonAsync("/api/duvidas",
            new PerguntarRequest(null, "Pergunta qualquer"))).LerJsonAsync<DuvidaDto>();

        var resposta = await cliente.PostarJsonAsync($"/api/duvidas/{duvida!.Id}/revisao", new MarcarRevisaoRequest(true));
        resposta.EnsureSuccessStatusCode();
        var atualizada = await resposta.LerJsonAsync<DuvidaDto>();

        Assert.True(atualizada!.MarcadaParaRevisao);
    }

    [Fact]
    public async Task Perguntar_TopicoInexistente_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/duvidas", new PerguntarRequest(999999, "Pergunta sobre tópico inexistente"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
