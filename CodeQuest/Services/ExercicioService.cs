using System.Text.Json;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>
/// Desfecho de uma tentativa: o parecer da IA + o que o BACKEND decidiu a partir dele —
/// aprovação (RN06), XP pela tabela (RN09, só se aprovado) e revisão agendada (RN06, se reprovado).
/// </summary>
public sealed record ResultadoTentativa(
    Tentativa Tentativa,
    CorrecaoIa Correcao,
    bool Aprovado,
    ResultadoXp? Recompensa,
    DateOnly? RevisaoAgendadaPara);

/// <summary>
/// Exercícios gerados e corrigidos pela IA (RF14, RF15, RF18 — Fase 2 do roadmap).
/// A IA é fonte de CONTEÚDO; toda decisão de jogo (aprovação, XP, revisão, nível do tópico)
/// é das regras do backend.
/// </summary>
public interface IExercicioService
{
    /// <summary>Gera um exercício por tópico + dificuldade (RF14) e persiste integralmente (RF18).</summary>
    Task<Resultado<Exercicio>> GerarAsync(int topicoId, Dificuldade dificuldade, FormatoExercicio formato,
        CancellationToken ct = default);

    /// <summary>
    /// Corrige a resposta via IA com nota 0–100 (RF15), agenda revisão se reprovado (RN06),
    /// recalcula o nível do tópico (RN07) e credita XP da tabela se aprovado (RN09).
    /// </summary>
    Task<Resultado<ResultadoTentativa>> ResponderAsync(int exercicioId, string resposta,
        CancellationToken ct = default);
}

public sealed class ExercicioService : IExercicioService
{
    private const int MaxExerciciosRecentes = 10; // títulos que entram no prompt como "não repetir"
    private const int MaxErrosRecentes = 10;
    private const int JanelaHistoricoDias = 30;   // janela do RN07 e dos erros recentes

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly AppDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly IGameService _game;
    private readonly ITabelaRecompensas _tabela;
    private readonly IPoliticaRevisao _revisao;
    private readonly ICalculadoraNivelTopico _nivelTopico;
    private readonly IRelogio _relogio;

    public ExercicioService(
        AppDbContext db,
        IGeminiClient gemini,
        IGameService game,
        ITabelaRecompensas tabela,
        IPoliticaRevisao revisao,
        ICalculadoraNivelTopico nivelTopico,
        IRelogio relogio)
    {
        _db = db;
        _gemini = gemini;
        _game = game;
        _tabela = tabela;
        _revisao = revisao;
        _nivelTopico = nivelTopico;
        _relogio = relogio;
    }

    public async Task<Resultado<Exercicio>> GerarAsync(int topicoId, Dificuldade dificuldade,
        FormatoExercicio formato, CancellationToken ct = default)
    {
        var topico = await _db.Topicos.Include(t => t.Modulo)
            .FirstOrDefaultAsync(t => t.Id == topicoId && !t.Arquivado, ct);
        if (topico is null)
            return Resultado.Falha<Exercicio>("Tópico não encontrado ou arquivado.");

        // Os 3 ingredientes anti-genérico (seção 1 do motor-ia): rubrica (no system prompt),
        // contexto do aluno (histórico abaixo) e cenário concreto sorteado.
        var exerciciosRecentes = await TitulosRecentesAsync(topicoId, ct);
        var errosRecentes = await ErrosRecentesAsync(topicoId, ct);
        var cenario = Cenarios.Sortear();

        var userPrompt = Prompts.UserGeracao(
            topico.Nome, topico.Modulo.Nome, dificuldade, formato,
            topico.NivelEstimado, errosRecentes, exerciciosRecentes, cenario);

        ExercicioIa gerado;
        try
        {
            gerado = await _gemini.GerarAsync<ExercicioIa>(
                Prompts.SystemGeracao, userPrompt, Prompts.ExercicioSchema,
                Prompts.TemperaturaGeracao, ct);
        }
        catch (IaIndisponivelException ex)
        {
            // RNF04/RNF07 — fallback amigável; o resto do jogo segue funcionando sem IA.
            return Resultado.Falha<Exercicio>($"{ex.Message} Enquanto isso, missões manuais seguem disponíveis.");
        }

        var exercicio = new Exercicio
        {
            TopicoId = topico.Id,
            Dificuldade = dificuldade,
            Formato = formato,
            // Enunciado público separado do gabarito: o front NUNCA recebe solução, problemas
            // plantados ou alternativa correta — só o que o aluno pode ver antes de responder.
            // Exceção deliberada: no Refactor os casos de teste SÃO públicos (o contrato que a
            // melhoria não pode quebrar, seção 8 do motor-ia).
            EnunciadoJson = JsonSerializer.Serialize(new
            {
                titulo = gerado.Titulo,
                enunciado = gerado.Enunciado,
                codigoPartida = gerado.CodigoPartida,
                codigoApresentado = gerado.CodigoApresentado,
                comportamentoEsperado = gerado.ComportamentoEsperado,
                casosDeTeste = formato == FormatoExercicio.Refactor ? gerado.CasosDeTeste : null,
                alternativas = gerado.Alternativas?.Select(a => new { a.Texto }),
                dicas = gerado.Dicas,
                cenario,
            }, JsonOpts),
            GabaritoJson = JsonSerializer.Serialize(gerado, JsonOpts), // completo (RF18)
            CriadoEm = _relogio.Agora,
        };
        _db.Exercicios.Add(exercicio);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(exercicio);
    }

    public async Task<Resultado<ResultadoTentativa>> ResponderAsync(int exercicioId, string resposta,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(resposta))
            return Resultado.Falha<ResultadoTentativa>("Resposta é obrigatória.");

        var exercicio = await _db.Exercicios.Include(e => e.Topico)
            .FirstOrDefaultAsync(e => e.Id == exercicioId, ct);
        if (exercicio is null)
            return Resultado.Falha<ResultadoTentativa>("Exercício não encontrado.");

        CorrecaoIa correcao;
        try
        {
            // Critério de correção específico do formato (seção 8): no CodeReview, por exemplo,
            // a nota é a proporção dos problemas plantados que o aluno encontrou.
            correcao = await _gemini.GerarAsync<CorrecaoIa>(
                Prompts.SystemCorrecaoPara(exercicio.Formato),
                Prompts.UserCorrecao(exercicio.GabaritoJson, resposta),
                Prompts.CorrecaoSchema, Prompts.TemperaturaCorrecao, ct);
        }
        catch (IaIndisponivelException ex)
        {
            return Resultado.Falha<ResultadoTentativa>(ex.Message);
        }

        var nota = Math.Clamp(correcao.Nota, 0, 100); // defesa contra IA fora da faixa (CHECK no banco)
        var aprovado = _revisao.Aprovado(nota);        // RN06 — o backend decide, não o campo "aprovado" da IA

        var tentativa = new Tentativa
        {
            ExercicioId = exercicio.Id,
            Resposta = resposta,
            Nota = nota,
            FeedbackJson = JsonSerializer.Serialize(correcao, JsonOpts), // persistido integralmente (RF18)
            Data = _relogio.Agora,
        };
        _db.Tentativas.Add(tentativa);

        // RN06 — reprovado entra na fila de repetição espaçada (+1, +3, +7 dias).
        DateOnly? revisaoPara = null;
        if (!aprovado)
        {
            var ciclosJaFeitos = await _db.Revisoes.CountAsync(r => r.Tentativa.ExercicioId == exercicio.Id, ct);
            if (_revisao.AposReprovar(ciclosJaFeitos, _relogio.Hoje) is { } proxima)
            {
                tentativa.Revisao = new Revisao { Ciclo = proxima.Ciclo, AgendadaPara = proxima.AgendadaPara };
                revisaoPara = proxima.AgendadaPara;
            }
        }

        // RN07 — nível do tópico: último teste (peso 3) + exercícios dos últimos 30 dias (peso 1).
        var corte = _relogio.Agora.AddDays(-JanelaHistoricoDias);
        var notas30Dias = await _db.Tentativas
            .Where(t => t.Exercicio.TopicoId == exercicio.TopicoId && t.Data >= corte)
            .Select(t => t.Nota)
            .ToListAsync(ct);
        notas30Dias.Add(nota); // a tentativa atual ainda não está no banco
        var notaUltimoTeste = await _db.Testes
            .Where(t => t.TopicoId == exercicio.TopicoId && t.Nota != null)
            .OrderByDescending(t => t.Data)
            .Select(t => t.Nota)
            .FirstOrDefaultAsync(ct);
        exercicio.Topico.NivelEstimado = _nivelTopico.Calcular(notaUltimoTeste, notas30Dias);

        await _db.SaveChangesAsync(ct);

        // RN09 — XP sai da tabela do backend, nunca da IA; e só quando aprovado (RN06).
        ResultadoXp? recompensa = null;
        if (aprovado)
            recompensa = await _game.AdicionarXpAsync(_tabela.XpExercicio(exercicio.Dificuldade), ct);

        return Resultado.Ok(new ResultadoTentativa(tentativa, correcao, aprovado, recompensa, revisaoPara));
    }

    /// <summary>Títulos dos últimos exercícios do tópico — entram no prompt como "NÃO REPETIR".</summary>
    private async Task<IReadOnlyList<string>> TitulosRecentesAsync(int topicoId, CancellationToken ct)
    {
        var enunciados = await _db.Exercicios
            .Where(e => e.TopicoId == topicoId)
            .OrderByDescending(e => e.CriadoEm)
            .Take(MaxExerciciosRecentes)
            .Select(e => e.EnunciadoJson)
            .ToListAsync(ct);

        return enunciados.Select(TituloDe).OfType<string>().ToList();
    }

    private static string? TituloDe(string enunciadoJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(enunciadoJson);
            return doc.RootElement.TryGetProperty("titulo", out var titulo) ? titulo.GetString() : null;
        }
        catch (JsonException)
        {
            return null; // enunciado antigo/corrompido não derruba a geração
        }
    }

    /// <summary>Conceitos que a IA apontou como fracos nas reprovações recentes do tópico — personalizam o prompt.</summary>
    private async Task<IReadOnlyList<string>> ErrosRecentesAsync(int topicoId, CancellationToken ct)
    {
        var corte = _relogio.Agora.AddDays(-JanelaHistoricoDias);
        var feedbacks = await _db.Tentativas
            .Where(t => t.Exercicio.TopicoId == topicoId
                && t.Nota < PoliticaRevisao.NotaMinima
                && t.Data >= corte
                && t.FeedbackJson != null)
            .OrderByDescending(t => t.Data)
            .Take(MaxErrosRecentes)
            .Select(t => t.FeedbackJson!)
            .ToListAsync(ct);

        return feedbacks
            .Select(f =>
            {
                try { return JsonSerializer.Deserialize<CorrecaoIa>(f, JsonOpts); }
                catch (JsonException) { return null; }
            })
            .Where(c => c is not null)
            .SelectMany(c => c!.ConceitosParaRevisar)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxErrosRecentes)
            .ToList();
    }
}
