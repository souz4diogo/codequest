using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class MissaoTests
{
    private readonly CodeQuestApiFactory _factory;

    public MissaoTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Criar_XpDentroDaFaixaDoEsforco_Sucesso()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Ler sobre LINQ", EsforcoMissao.Rapida, 15));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var missao = await resposta.LerJsonAsync<MissaoDto>();
        Assert.Equal(15, missao!.XpRecompensa);
        Assert.Equal(TipoMissao.Manual, missao.Tipo);
        Assert.Equal(StatusMissao.Pendente, missao.Status);
    }

    [Fact]
    public async Task Criar_XpForaDaFaixaDoEsforco_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        // Rápida permite 10-20; 90 está fora da faixa (RN04).
        var resposta = await cliente.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Tentando trapacear", EsforcoMissao.Rapida, 90));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Faixas_DevolveAsTresFaixasFixas()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var faixas = await cliente.ObterJsonAsync<List<FaixaEsforcoDto>>("/api/missoes/faixas");

        Assert.NotNull(faixas);
        Assert.Equal(3, faixas!.Count);
        Assert.Contains(faixas, f => f.Esforco == EsforcoMissao.Rapida && f.XpMinimo == 10 && f.XpMaximo == 20);
        Assert.Contains(faixas, f => f.Esforco == EsforcoMissao.Longa && f.XpMinimo == 60 && f.XpMaximo == 100);
    }

    [Fact]
    public async Task Concluir_CreditaXpEGoldEColoqueMissaoComoConcluida()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        var criada = await (await cliente.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Revisar PR", EsforcoMissao.Media, 40))).LerJsonAsync<MissaoDto>();

        var respostaConcluir = await cliente.PostAsync($"/api/missoes/{criada!.Id}/concluir", null);
        respostaConcluir.EnsureSuccessStatusCode();
        var resultado = await respostaConcluir.LerJsonAsync<ResultadoXpDto>();

        Assert.NotNull(resultado);
        Assert.True(resultado!.XpCreditado > 0);

        var player = await cliente.ObterJsonAsync<PlayerDto>("/api/player");
        Assert.Equal(resultado.XpCreditado, player!.XpTotal);
    }

    [Fact]
    public async Task Concluir_MesmaMissaoDuasVezes_SegundaRetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        var criada = await (await cliente.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Estudar EF Core", EsforcoMissao.Rapida, 10))).LerJsonAsync<MissaoDto>();

        var primeira = await cliente.PostAsync($"/api/missoes/{criada!.Id}/concluir", null);
        primeira.EnsureSuccessStatusCode();

        var segunda = await cliente.PostAsync($"/api/missoes/{criada.Id}/concluir", null);

        Assert.Equal(HttpStatusCode.BadRequest, segunda.StatusCode);
    }

    [Fact]
    public async Task Concluir_MissaoDeOutroUsuario_RetornaBadRequest()
    {
        var (clienteA, _) = await _factory.RegistrarELogarAsync();
        var (clienteB, _) = await _factory.RegistrarELogarAsync();

        var missaoDeA = await (await clienteA.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Missão da A", EsforcoMissao.Rapida, 10))).LerJsonAsync<MissaoDto>();

        var resposta = await clienteB.PostAsync($"/api/missoes/{missaoDeA!.Id}/concluir", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ListarDoDia_SoIncluiMissoesDoProprioUsuario()
    {
        var (clienteA, _) = await _factory.RegistrarELogarAsync();
        var (clienteB, _) = await _factory.RegistrarELogarAsync();

        await clienteA.PostarJsonAsync("/api/missoes", new CriarMissaoRequest("Só da A", EsforcoMissao.Rapida, 10));

        var missoesB = await clienteB.ObterJsonAsync<List<MissaoDto>>("/api/missoes/dia");

        Assert.DoesNotContain(missoesB!, m => m.Titulo == "Só da A");
    }
}
