using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Detalhamento do que aconteceu ao creditar XP — usado pela UI para feedback (level-up etc.).</summary>
/// <param name="XpBase">XP antes do multiplicador de streak.</param>
/// <param name="XpCreditado">XP efetivamente somado (após streak).</param>
/// <param name="GoldGanho">Gold derivado do XP creditado.</param>
/// <param name="Multiplicador">Multiplicador de streak aplicado.</param>
/// <param name="SubiuNivel">Se a atividade causou level-up.</param>
/// <param name="NivelAtual">Nível após a atividade.</param>
/// <param name="StreakAtual">Streak após a atividade.</param>
public readonly record struct ResultadoXp(
    int XpBase,
    int XpCreditado,
    int GoldGanho,
    double Multiplicador,
    bool SubiuNivel,
    int NivelAtual,
    int StreakAtual);

/// <summary>
/// Coração da gamificação (RN01–RN05). É o único lugar onde XP/nível/gold/streak mudam.
/// Depende apenas de abstrações das regras (DIP), então a política de cada regra pode
/// evoluir isoladamente sem alterar esta orquestração.
/// </summary>
public interface IGameService
{
    Task<Player> ObterPlayerAsync(CancellationToken ct = default);

    /// <summary>
    /// Credita XP por uma atividade pontuável: atualiza streak, aplica multiplicador,
    /// converte em gold, recalcula o nível e persiste. RN09: o XP base vem sempre do
    /// backend, nunca da IA.
    /// </summary>
    Task<ResultadoXp> AdicionarXpAsync(int xpBase, CancellationToken ct = default);
}

public sealed class GameService : IGameService
{
    private readonly AppDbContext _db;
    private readonly IReguladorNivel _nivel;
    private readonly ICalculadoraRecompensa _recompensa;
    private readonly IPoliticaStreak _streak;
    private readonly IRelogio _relogio;

    public GameService(
        AppDbContext db,
        IReguladorNivel nivel,
        ICalculadoraRecompensa recompensa,
        IPoliticaStreak streak,
        IRelogio relogio)
    {
        _db = db;
        _nivel = nivel;
        _recompensa = recompensa;
        _streak = streak;
        _relogio = relogio;
    }

    public async Task<Player> ObterPlayerAsync(CancellationToken ct = default)
        => await _db.Players.FirstOrDefaultAsync(p => p.Id == Player.IdUnico, ct)
           ?? throw new InvalidOperationException("Player não encontrado — rode o seed do banco.");

    public async Task<ResultadoXp> AdicionarXpAsync(int xpBase, CancellationToken ct = default)
    {
        if (xpBase < 0) throw new ArgumentOutOfRangeException(nameof(xpBase));

        var player = await ObterPlayerAsync(ct);

        // 1) Streak primeiro — o multiplicador de hoje usa o streak já atualizado.
        var streak = _streak.Avaliar(
            player.StreakDias, player.UltimoDiaAtivo, _relogio.Hoje, player.PocaoStreakAtiva);

        player.StreakDias = streak.NovoStreak;
        player.UltimoDiaAtivo = _relogio.Hoje;
        if (streak.ConsumiuPocao)
            player.PocaoStreakAtiva = false;

        // 2) XP com multiplicador de streak (RN03).
        var xpCreditado = _recompensa.AplicarMultiplicador(xpBase, player.StreakDias);
        player.XpTotal += xpCreditado;

        // 3) Gold derivado do XP creditado (RN02).
        var gold = _recompensa.GoldPorXp(xpCreditado);
        player.Gold += gold;

        // 4) Recalcula nível (RN01).
        var nivelAntigo = player.Nivel;
        player.Nivel = _nivel.CalcularNivel(player.XpTotal);

        await _db.SaveChangesAsync(ct);

        return new ResultadoXp(
            XpBase: xpBase,
            XpCreditado: xpCreditado,
            GoldGanho: gold,
            Multiplicador: _recompensa.MultiplicadorStreak(player.StreakDias),
            SubiuNivel: player.Nivel > nivelAntigo,
            NivelAtual: player.Nivel,
            StreakAtual: player.StreakDias);
    }
}
