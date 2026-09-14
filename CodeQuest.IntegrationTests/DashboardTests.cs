using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class DashboardTests
{
    private readonly CodeQuestApiFactory _factory;

    public DashboardTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Atividade_MissaoConcluidaHoje_ApareceNoDiaAtual()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();
        var missao = await (await cliente.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Estudar hoje", EsforcoMissao.Rapida, 10))).LerJsonAsync<MissaoDto>();
        await cliente.PostAsync($"/api/missoes/{missao!.Id}/concluir", null);

        var atividade = await cliente.ObterJsonAsync<List<AtividadeDoDiaDto>>("/api/dashboard/atividade");

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        Assert.NotNull(atividade);
        Assert.Contains(atividade!, a => a.Data == hoje && a.Quantidade >= 1);
    }

    /// <summary>
    /// Missão/SessaoFoco são player-scoped, mas Exercicio/Teste NÃO são (globais nesta fase,
    /// ver DashboardService) — outros testes da suíte já geram atividade "global" no dia de
    /// hoje. Por isso a comparação é por delta (antes/depois), não por ausência absoluta.
    /// </summary>
    [Fact]
    public async Task Atividade_MissaoDeOutroUsuario_NaoAumentaMinhaContagemDeHoje()
    {
        var (clienteA, _) = await _factory.RegistrarELogarAsync();
        var (clienteB, _) = await _factory.RegistrarELogarAsync();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        var antes = await clienteB.ObterJsonAsync<List<AtividadeDoDiaDto>>("/api/dashboard/atividade");
        var quantidadeAntes = antes!.First(a => a.Data == hoje).Quantidade;

        var missaoDeA = await (await clienteA.PostarJsonAsync("/api/missoes",
            new CriarMissaoRequest("Missão da A", EsforcoMissao.Rapida, 10))).LerJsonAsync<MissaoDto>();
        await clienteA.PostAsync($"/api/missoes/{missaoDeA!.Id}/concluir", null);

        var depois = await clienteB.ObterJsonAsync<List<AtividadeDoDiaDto>>("/api/dashboard/atividade");
        var quantidadeDepois = depois!.First(a => a.Data == hoje).Quantidade;

        Assert.Equal(quantidadeAntes, quantidadeDepois);
    }

    [Fact]
    public async Task Atividade_CobreOsUltimos30Dias()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var atividade = await cliente.ObterJsonAsync<List<AtividadeDoDiaDto>>("/api/dashboard/atividade");

        Assert.NotNull(atividade);
        Assert.Equal(30, atividade!.Count);
    }

    [Fact]
    public async Task Radar_ReflenteNivelMedioDosTopicosDoModulo()
    {
        var (modulo, _) = await _factory.CriarModuloComTopicoAsync(nivelTopico: 40);
        await using (var db = _factory.CriarDbContext())
        {
            db.Topicos.Add(new CodeQuest.Models.Topico { ModuloId = modulo.Id, Nome = "Tópico 2", NivelEstimado = 60 });
            await db.SaveChangesAsync();
        }
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var radar = await cliente.ObterJsonAsync<List<NivelPorModuloDto>>("/api/dashboard/radar");

        Assert.Contains(radar!, r => r.Modulo == modulo.Nome && r.NivelMedio == 50);
    }
}
