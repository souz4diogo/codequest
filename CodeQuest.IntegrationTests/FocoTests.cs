using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class FocoTests
{
    private readonly CodeQuestApiFactory _factory;

    public FocoTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Registrar_SessaoCompleta_CreditaXpEMarcaCompletou()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/foco",
            new RegistrarFocoRequest(topico.Id, MinutosPlanejados: 25, MinutosReais: 25));
        resposta.EnsureSuccessStatusCode();
        var sessao = await resposta.LerJsonAsync<SessaoFocoDto>();

        Assert.True(sessao!.Completou);
        Assert.True(sessao.XpGanho > 0);
    }

    [Fact]
    public async Task Registrar_SessaoIncompleta_NaoCreditaXp()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/foco",
            new RegistrarFocoRequest(topico.Id, MinutosPlanejados: 25, MinutosReais: 10));
        resposta.EnsureSuccessStatusCode();
        var sessao = await resposta.LerJsonAsync<SessaoFocoDto>();

        Assert.False(sessao!.Completou);
        Assert.Equal(0, sessao.XpGanho);
    }

    [Fact]
    public async Task Registrar_TopicoInexistente_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/foco",
            new RegistrarFocoRequest(999999, MinutosPlanejados: 25, MinutosReais: 25));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Registrar_DuracaoPlanejadaZeroOuNegativa_RetornaBadRequest()
    {
        var (_, topico) = await _factory.CriarModuloComTopicoAsync();
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/foco",
            new RegistrarFocoRequest(topico.Id, MinutosPlanejados: 0, MinutosReais: 0));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
