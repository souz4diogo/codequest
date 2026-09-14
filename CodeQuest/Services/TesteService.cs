using System.Text.Json;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Desfecho de um teste de tópico corrigido.</summary>
public sealed record ResultadoTeste(Teste Teste, CorrecaoTeste Correcao);

/// <summary>Teste de avaliação por tópico (RF17): gerado pela IA, atualiza o nível do tópico (RN07)
/// e registra gaps quando corrigido.</summary>
public interface ITesteService
{
    Task<Resultado<Teste>> IniciarAsync(int topicoId, Dificuldade dificuldade, CancellationToken ct = default);

    Task<Resultado<ResultadoTeste>> ResponderAsync(int testeId,
        IReadOnlyList<int> respostas, CancellationToken ct = default);
}

public sealed class TesteService : ITesteService
{
    private const int QuantidadeQuestoes = 7;
    private const int JanelaHistoricoDias = 30; // mesma janela do RN07 usada em exercícios

    private readonly AppDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly ICalculadoraNivelTopico _nivelTopico;
    private readonly IRelogio _relogio;

    public TesteService(AppDbContext db, IGeminiClient gemini, ICalculadoraNivelTopico nivelTopico, IRelogio relogio)
    {
        _db = db;
        _gemini = gemini;
        _nivelTopico = nivelTopico;
        _relogio = relogio;
    }

    public async Task<Resultado<Teste>> IniciarAsync(int topicoId, Dificuldade dificuldade, CancellationToken ct = default)
    {
        var topico = await _db.Topicos.Include(t => t.Modulo)
            .FirstOrDefaultAsync(t => t.Id == topicoId && !t.Arquivado, ct);
        if (topico is null)
            return Resultado.Falha<Teste>("Tópico não encontrado ou arquivado.");

        TesteIa gerado;
        try
        {
            gerado = await _gemini.GerarAsync<TesteIa>(
                Prompts.SystemGeracaoTeste,
                Prompts.UserGeracaoTeste(topico.Nome, topico.Modulo.Nome, dificuldade, topico.NivelEstimado, QuantidadeQuestoes),
                Prompts.TesteSchema, Prompts.TemperaturaGeracao, ct);
        }
        catch (IaIndisponivelException ex)
        {
            return Resultado.Falha<Teste>($"{ex.Message} Enquanto isso, missões manuais seguem disponíveis.");
        }

        var teste = new Teste
        {
            TopicoId = topico.Id,
            Dificuldade = dificuldade,
            QuestoesJson = JsonSerializer.Serialize(gerado, JsonPadrao.Opcoes),
            Data = _relogio.Agora,
        };
        _db.Testes.Add(teste);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(teste);
    }

    public async Task<Resultado<ResultadoTeste>> ResponderAsync(int testeId,
        IReadOnlyList<int> respostas, CancellationToken ct = default)
    {
        var teste = await _db.Testes.Include(t => t.Topico)
            .FirstOrDefaultAsync(t => t.Id == testeId && t.TopicoId != null, ct);
        if (teste is null)
            return Resultado.Falha<ResultadoTeste>("Teste não encontrado.");
        if (teste.Nota is not null)
            return Resultado.Falha<ResultadoTeste>("Teste já foi corrigido.");

        var gabarito = JsonSerializer.Deserialize<TesteIa>(teste.QuestoesJson, JsonPadrao.Opcoes)!;
        var correcao = CorretorTeste.Corrigir(gabarito, respostas);

        teste.RespostasJson = JsonSerializer.Serialize(respostas, JsonPadrao.Opcoes);
        teste.Nota = correcao.Nota;
        teste.GapsJson = JsonSerializer.Serialize(correcao.Gaps, JsonPadrao.Opcoes);

        // RN07 — nível do tópico: este teste vira o "último teste" (peso 3) + exercícios dos últimos 30 dias.
        var corte = _relogio.Agora.AddDays(-JanelaHistoricoDias);
        var notas30Dias = await _db.Tentativas
            .Where(t => t.Exercicio.TopicoId == teste.TopicoId && t.Data >= corte)
            .Select(t => t.Nota)
            .ToListAsync(ct);
        teste.Topico!.NivelEstimado = _nivelTopico.Calcular(correcao.Nota, notas30Dias);

        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(new ResultadoTeste(teste, correcao));
    }
}
