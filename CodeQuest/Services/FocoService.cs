using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>
/// Sessão de foco Pomodoro (RF19). 1 XP por minuto planejado, só creditado se o timer completou
/// (MinutosReais ≥ MinutosPlanejados) — trapacear desligando o timer cedo não rende nada.
/// </summary>
public interface IFocoService
{
    Task<Resultado<SessaoFoco>> RegistrarAsync(int topicoId, int minutosPlanejados, int minutosReais,
        CancellationToken ct = default);
}

public sealed class FocoService : IFocoService
{
    private readonly AppDbContext _db;
    private readonly IGameService _game;
    private readonly IUsuarioAtual _usuario;
    private readonly IRelogio _relogio;

    public FocoService(AppDbContext db, IGameService game, IUsuarioAtual usuario, IRelogio relogio)
    {
        _db = db;
        _game = game;
        _usuario = usuario;
        _relogio = relogio;
    }

    public async Task<Resultado<SessaoFoco>> RegistrarAsync(int topicoId, int minutosPlanejados,
        int minutosReais, CancellationToken ct = default)
    {
        if (minutosPlanejados <= 0 || minutosReais < 0)
            return Resultado.Falha<SessaoFoco>("Duração inválida.");

        var topico = await _db.Topicos.FirstOrDefaultAsync(t => t.Id == topicoId && !t.Arquivado, ct);
        if (topico is null)
            return Resultado.Falha<SessaoFoco>("Tópico não encontrado ou arquivado.");

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var completou = minutosReais >= minutosPlanejados;

        var sessao = new SessaoFoco
        {
            PlayerId = playerId,
            TopicoId = topico.Id,
            MinutosPlanejados = minutosPlanejados,
            MinutosReais = minutosReais,
            XpGanho = 0,
            Data = _relogio.Agora,
        };

        if (completou)
        {
            var recompensa = await _game.AdicionarXpAsync(minutosPlanejados, ct);
            sessao.XpGanho = recompensa.XpCreditado;
        }

        _db.SessoesFoco.Add(sessao);
        await _db.SaveChangesAsync(ct);

        return Resultado.Ok(sessao);
    }
}
