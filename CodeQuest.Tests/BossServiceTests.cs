using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using CodeQuest.Tests.Suporte;
using Xunit;

namespace CodeQuest.Tests;

public class BossServiceTests
{
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

    private static async Task<(Modulo modulo, Topico topico)> CriarModuloComTopicoAsync(
        AppDbContext db, int nivelTopico, StatusModulo status = StatusModulo.Liberado)
    {
        var modulo = new Modulo { Nome = "Módulo X", Status = status };
        db.Modulos.Add(modulo);
        await db.SaveChangesAsync();

        var topico = new Topico { ModuloId = modulo.Id, Nome = "Tópico 1", NivelEstimado = nivelTopico };
        db.Topicos.Add(topico);
        await db.SaveChangesAsync();

        return (modulo, topico);
    }

    [Fact]
    public async Task IniciarAsync_ModuloInexistente_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.IniciarAsync(999);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task IniciarAsync_ModuloBloqueado_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: 60, status: StatusModulo.Bloqueado);
        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.IniciarAsync(modulo.Id);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task IniciarAsync_TopicoAbaixoDoNivelMinimo_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico - 1);
        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.IniciarAsync(modulo.Id);

        Assert.False(resultado.Sucesso);
        Assert.Contains("liberado", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task IniciarAsync_TopicosNoNivelMinimo_CriaTesteDeBoss()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(2)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.IniciarAsync(modulo.Id);

        Assert.True(resultado.Sucesso);
        Assert.Equal(modulo.Id, resultado.Valor!.ModuloId);
        Assert.Equal(Dificuldade.Expert, resultado.Valor.Dificuldade);
    }

    [Fact]
    public async Task IniciarAsync_IaIndisponivel_RetornaFalhaSemQuebrar()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var service = new BossService(db, GeminiClientFake.Lancando(new IaIndisponivelException("IA fora do ar.")),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.IniciarAsync(modulo.Id);

        Assert.False(resultado.Sucesso);
        Assert.Contains("missões manuais", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResponderAsync_NotaAbaixoDoMinimo_NaoConcluiModulo()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var teste = new Teste
        {
            ModuloId = modulo.Id,
            Dificuldade = Dificuldade.Expert,
            QuestoesJson = System.Text.Json.JsonSerializer.Serialize(TesteIaComQuestoes(2, alternativaCorretaIndice: 0)),
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(2)),
            new PoliticaArvore(), new RelogioFake());

        // erra as duas -> nota 0
        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 1, 1 });

        Assert.True(resultado.Sucesso);
        Assert.False(resultado.Valor!.Aprovado);
        Assert.Equal(StatusModulo.Liberado, modulo.Status);
        Assert.Empty(resultado.Valor.ModulosLiberados);
    }

    [Fact]
    public async Task ResponderAsync_NotaAcimaDoMinimo_ConcluiModuloELiberaDependentes()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);

        var dependente = new Modulo { Nome = "Módulo Y", Status = StatusModulo.Bloqueado };
        db.Modulos.Add(dependente);
        await db.SaveChangesAsync();
        db.ModuloPrereqs.Add(new ModuloPrereq { ModuloId = dependente.Id, RequerModuloId = modulo.Id });
        await db.SaveChangesAsync();

        var teste = new Teste
        {
            ModuloId = modulo.Id,
            Dificuldade = Dificuldade.Expert,
            QuestoesJson = System.Text.Json.JsonSerializer.Serialize(TesteIaComQuestoes(2, alternativaCorretaIndice: 0)),
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(2)),
            new PoliticaArvore(), new RelogioFake());

        // acerta as duas -> nota 100
        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 0, 0 });

        Assert.True(resultado.Sucesso);
        Assert.True(resultado.Valor!.Aprovado);
        Assert.Equal(StatusModulo.Concluido, modulo.Status);
        Assert.Equal(100, modulo.NotaBoss);
        Assert.Contains("Módulo Y", resultado.Valor.ModulosLiberados);
        Assert.Equal(StatusModulo.Liberado, dependente.Status);
    }

    [Fact]
    public async Task ResponderAsync_TesteJaCorrigido_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var (modulo, _) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var teste = new Teste
        {
            ModuloId = modulo.Id,
            Dificuldade = Dificuldade.Expert,
            QuestoesJson = System.Text.Json.JsonSerializer.Serialize(TesteIaComQuestoes(1)),
            Nota = 80,
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 0 });

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ResponderAsync_TesteDeTopicoNaoDeBoss_NaoEncontrado()
    {
        using var db = DbContextFactory.Criar();
        var (_, topico) = await CriarModuloComTopicoAsync(db, nivelTopico: PoliticaArvore.NivelMinimoTopico);
        var testeDeTopico = new Teste
        {
            TopicoId = topico.Id,
            Dificuldade = Dificuldade.Easy,
            QuestoesJson = System.Text.Json.JsonSerializer.Serialize(TesteIaComQuestoes(1)),
        };
        db.Testes.Add(testeDeTopico);
        await db.SaveChangesAsync();

        var service = new BossService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new PoliticaArvore(), new RelogioFake());

        var resultado = await service.ResponderAsync(testeDeTopico.Id, new List<int> { 0 });

        Assert.False(resultado.Sucesso);
    }
}
