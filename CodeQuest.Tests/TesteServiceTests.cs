using System.Text.Json;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using CodeQuest.Tests.Suporte;
using Xunit;

namespace CodeQuest.Tests;

public class TesteServiceTests
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

    private static async Task<Topico> CriarTopicoAsync(AppDbContext db, int nivelInicial = 0, bool arquivado = false)
    {
        var modulo = new Modulo { Nome = "Módulo X" };
        db.Modulos.Add(modulo);
        await db.SaveChangesAsync();

        var topico = new Topico
        {
            ModuloId = modulo.Id,
            Nome = "Tópico 1",
            NivelEstimado = nivelInicial,
            Arquivado = arquivado,
        };
        db.Topicos.Add(topico);
        await db.SaveChangesAsync();
        return topico;
    }

    [Fact]
    public async Task IniciarAsync_TopicoInexistente_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.IniciarAsync(999, Dificuldade.Medium);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task IniciarAsync_TopicoArquivado_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db, arquivado: true);
        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.IniciarAsync(topico.Id, Dificuldade.Medium);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task IniciarAsync_TopicoValido_CriaTesteComDificuldadeEscolhida()
    {
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db);
        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(3)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.IniciarAsync(topico.Id, Dificuldade.Hard);

        Assert.True(resultado.Sucesso);
        Assert.Equal(topico.Id, resultado.Valor!.TopicoId);
        Assert.Equal(Dificuldade.Hard, resultado.Valor.Dificuldade);
        Assert.Null(resultado.Valor.Nota);
    }

    [Fact]
    public async Task IniciarAsync_IaIndisponivel_RetornaFalhaSemQuebrar()
    {
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db);
        var service = new TesteService(db, GeminiClientFake.Lancando(new IaIndisponivelException("IA fora do ar.")),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.IniciarAsync(topico.Id, Dificuldade.Medium);

        Assert.False(resultado.Sucesso);
        Assert.Contains("missões manuais", resultado.Erro, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResponderAsync_TesteInexistente_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.ResponderAsync(999, new List<int> { 0 });

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ResponderAsync_TesteJaCorrigido_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db);
        var teste = new Teste
        {
            TopicoId = topico.Id,
            Dificuldade = Dificuldade.Medium,
            QuestoesJson = JsonSerializer.Serialize(TesteIaComQuestoes(1)),
            Nota = 80,
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 0 });

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ResponderAsync_TesteDeBossNaoDeTopico_NaoEncontrado()
    {
        using var db = DbContextFactory.Criar();
        var modulo = new Modulo { Nome = "Módulo X" };
        db.Modulos.Add(modulo);
        await db.SaveChangesAsync();

        var testeDeBoss = new Teste
        {
            ModuloId = modulo.Id,
            Dificuldade = Dificuldade.Expert,
            QuestoesJson = JsonSerializer.Serialize(TesteIaComQuestoes(1)),
        };
        db.Testes.Add(testeDeBoss);
        await db.SaveChangesAsync();

        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.ResponderAsync(testeDeBoss.Id, new List<int> { 0 });

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task ResponderAsync_SemHistoricoDeExercicios_NivelViraApenasNotaDoTeste()
    {
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db, nivelInicial: 0);
        var teste = new Teste
        {
            TopicoId = topico.Id,
            Dificuldade = Dificuldade.Medium,
            QuestoesJson = JsonSerializer.Serialize(TesteIaComQuestoes(1, alternativaCorretaIndice: 0)),
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), new RelogioFake());

        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 0 }); // acerta -> nota 100

        Assert.True(resultado.Sucesso);
        Assert.Equal(100, resultado.Valor!.Correcao.Nota);
        Assert.Equal(100, topico.NivelEstimado);
    }

    [Fact]
    public async Task ResponderAsync_ComExerciciosRecentes_PonderaNotaDoTesteComExercicios()
    {
        var relogio = new RelogioFake();
        using var db = DbContextFactory.Criar();
        var topico = await CriarTopicoAsync(db, nivelInicial: 0);

        var exercicio = new Exercicio { TopicoId = topico.Id, Dificuldade = Dificuldade.Medium, Formato = FormatoExercicio.MultiplaEscolha };
        db.Exercicios.Add(exercicio);
        await db.SaveChangesAsync();

        // Tentativa dentro da janela de 30 dias, nota 40
        db.Tentativas.Add(new Tentativa { ExercicioId = exercicio.Id, Nota = 40, Data = relogio.Agora.AddDays(-5) });
        // Tentativa fora da janela (deve ser ignorada)
        db.Tentativas.Add(new Tentativa { ExercicioId = exercicio.Id, Nota = 0, Data = relogio.Agora.AddDays(-40) });
        await db.SaveChangesAsync();

        var teste = new Teste
        {
            TopicoId = topico.Id,
            Dificuldade = Dificuldade.Medium,
            QuestoesJson = JsonSerializer.Serialize(TesteIaComQuestoes(1, alternativaCorretaIndice: 0)),
        };
        db.Testes.Add(teste);
        await db.SaveChangesAsync();

        var service = new TesteService(db, GeminiClientFake.RetornandoOk(TesteIaComQuestoes(1)),
            new CalculadoraNivelTopico(), relogio);

        // acerta -> nota do teste 100; peso 3 (teste) + peso 1 (exercício nota 40) = (300 + 40) / 4 = 85
        var resultado = await service.ResponderAsync(teste.Id, new List<int> { 0 });

        Assert.True(resultado.Sucesso);
        Assert.Equal(85, topico.NivelEstimado);
    }
}
