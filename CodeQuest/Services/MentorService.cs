using System.Text.Json;
using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Ai;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>
/// Mentor IA (RF22/RF23): chat de dúvidas com contexto automático (tópico + nível do aluno),
/// persistindo cada pergunta/resposta e permitindo marcar para revisão futura.
/// </summary>
public interface IMentorService
{
    Task<Resultado<Duvida>> PerguntarAsync(int? topicoId, string pergunta, CancellationToken ct = default);

    Task<IReadOnlyList<Duvida>> ListarAsync(CancellationToken ct = default);

    Task<Resultado<Duvida>> MarcarParaRevisaoAsync(int duvidaId, bool marcada, CancellationToken ct = default);
}

public sealed class MentorService : IMentorService
{
    private const int MaxHistorico = 30;

    private readonly AppDbContext _db;
    private readonly IGeminiClient _gemini;
    private readonly IRelogio _relogio;
    private readonly IUsuarioAtual _usuario;

    public MentorService(AppDbContext db, IGeminiClient gemini, IRelogio relogio, IUsuarioAtual usuario)
    {
        _db = db;
        _gemini = gemini;
        _relogio = relogio;
        _usuario = usuario;
    }

    public async Task<Resultado<Duvida>> PerguntarAsync(int? topicoId, string pergunta, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(pergunta))
            return Resultado.Falha<Duvida>("Pergunta é obrigatória.");

        Topico? topico = null;
        if (topicoId is { } id)
        {
            topico = await _db.Topicos.FirstOrDefaultAsync(t => t.Id == id && !t.Arquivado, ct);
            if (topico is null)
                return Resultado.Falha<Duvida>("Tópico não encontrado ou arquivado.");
        }

        RespostaMentorIa resposta;
        try
        {
            resposta = await _gemini.GerarAsync<RespostaMentorIa>(
                Prompts.SystemMentor,
                Prompts.UserMentor(topico?.Nome, topico?.NivelEstimado, pergunta),
                Prompts.MentorSchema, Prompts.TemperaturaMentor, ct);
        }
        catch (IaIndisponivelException ex)
        {
            return Resultado.Falha<Duvida>($"{ex.Message} Mentor indisponível, missão manual liberada.");
        }

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var duvida = new Duvida
        {
            PlayerId = playerId,
            TopicoId = topico?.Id,
            Pergunta = pergunta,
            RespostaJson = JsonSerializer.Serialize(resposta, JsonPadrao.Opcoes),
            Data = _relogio.Agora,
        };
        _db.Duvidas.Add(duvida);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(duvida);
    }

    public async Task<IReadOnlyList<Duvida>> ListarAsync(CancellationToken ct = default)
    {
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        return await _db.Duvidas.Include(d => d.Topico)
            .Where(d => d.PlayerId == playerId)
            .OrderByDescending(d => d.Data)
            .Take(MaxHistorico)
            .ToListAsync(ct);
    }

    public async Task<Resultado<Duvida>> MarcarParaRevisaoAsync(int duvidaId, bool marcada, CancellationToken ct = default)
    {
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var duvida = await _db.Duvidas.Include(d => d.Topico)
            .FirstOrDefaultAsync(d => d.Id == duvidaId && d.PlayerId == playerId, ct);
        if (duvida is null)
            return Resultado.Falha<Duvida>("Dúvida não encontrada.");

        duvida.MarcadaParaRevisao = marcada;
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(duvida);
    }
}
