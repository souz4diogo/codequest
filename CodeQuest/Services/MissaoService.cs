using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Dados para criar uma missão manual (RF09).</summary>
/// <param name="Titulo">Título da missão.</param>
/// <param name="Esforco">Esforço estimado, define a faixa de XP (RN04).</param>
/// <param name="Xp">XP escolhido pelo usuário dentro da faixa.</param>
/// <param name="Descricao">Descrição opcional.</param>
/// <param name="TopicoId">Tópico foco opcional.</param>
public readonly record struct NovaMissaoManual(
    string Titulo,
    EsforcoMissao Esforco,
    int Xp,
    string? Descricao = null,
    int? TopicoId = null);

/// <summary>
/// Missões (RF09–RF13). Nesta fase cobre o fluxo manual: criar validando a faixa de XP
/// (RN04), derivar o gold (RN02) e, ao concluir, creditar as recompensas via GameService.
/// </summary>
public interface IMissaoService
{
    Task<Resultado<Missao>> CriarManualAsync(NovaMissaoManual dados, CancellationToken ct = default);

    /// <summary>Conclui uma missão pendente/em andamento e credita XP+gold ao jogador.</summary>
    Task<Resultado<ResultadoXp>> ConcluirAsync(int missaoId, CancellationToken ct = default);

    Task<IReadOnlyList<Missao>> ListarDoDiaAsync(CancellationToken ct = default);
}

public sealed class MissaoService : IMissaoService
{
    private readonly AppDbContext _db;
    private readonly IPoliticaRecompensaMissao _politica;
    private readonly ICalculadoraRecompensa _recompensa;
    private readonly IGameService _game;
    private readonly IRelogio _relogio;
    private readonly IUsuarioAtual _usuario;

    public MissaoService(
        AppDbContext db,
        IPoliticaRecompensaMissao politica,
        ICalculadoraRecompensa recompensa,
        IGameService game,
        IRelogio relogio,
        IUsuarioAtual usuario)
    {
        _db = db;
        _politica = politica;
        _recompensa = recompensa;
        _game = game;
        _relogio = relogio;
        _usuario = usuario;
    }

    public async Task<Resultado<Missao>> CriarManualAsync(NovaMissaoManual dados, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dados.Titulo))
            return Resultado.Falha<Missao>("Título é obrigatório.");

        // RN04 — XP precisa cair na faixa do esforço escolhido.
        if (!_politica.XpValido(dados.Esforco, dados.Xp))
        {
            var faixa = _politica.FaixaDe(dados.Esforco);
            return Resultado.Falha<Missao>(
                $"XP {dados.Xp} fora da faixa de esforço {dados.Esforco} ({faixa.Minimo}–{faixa.Maximo}).");
        }

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var missao = new Missao
        {
            PlayerId = playerId,
            Tipo = TipoMissao.Manual,
            Titulo = dados.Titulo.Trim(),
            Descricao = dados.Descricao?.Trim() ?? string.Empty,
            TopicoId = dados.TopicoId,
            XpRecompensa = dados.Xp,
            GoldRecompensa = _recompensa.GoldPorXp(dados.Xp), // RN02
            Status = StatusMissao.Pendente,
            DataAlvo = _relogio.Hoje,
        };

        _db.Missoes.Add(missao);
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(missao);
    }

    public async Task<Resultado<ResultadoXp>> ConcluirAsync(int missaoId, CancellationToken ct = default)
    {
        // Só o dono conclui a própria missão (isolamento entre usuários).
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        await ExpirarVencidasAsync(playerId, ct); // RN10: missão de ontem não pode mais ser concluída

        var missao = await _db.Missoes.FirstOrDefaultAsync(m => m.Id == missaoId && m.PlayerId == playerId, ct);
        if (missao is null)
            return Resultado.Falha<ResultadoXp>("Missão não encontrada.");

        if (missao.Status is StatusMissao.Concluida)
            return Resultado.Falha<ResultadoXp>("Missão já concluída.");
        if (missao.Status is StatusMissao.Expirada)
            return Resultado.Falha<ResultadoXp>("Missão expirada.");

        missao.Status = StatusMissao.Concluida;
        missao.ConcluidaEm = _relogio.Agora;

        // GameService é a única fonte de verdade sobre XP/gold/streak (RN09).
        var recompensa = await _game.AdicionarXpAsync(missao.XpRecompensa, ct);

        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(recompensa);
    }

    public async Task<IReadOnlyList<Missao>> ListarDoDiaAsync(CancellationToken ct = default)
    {
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        await ExpirarVencidasAsync(playerId, ct);

        return await _db.Missoes
            .Where(m => m.PlayerId == playerId && m.DataAlvo == _relogio.Hoje)
            .OrderBy(m => m.Status)
            .ToListAsync(ct);
    }

    /// <summary>
    /// RN10 — missões abertas de dias anteriores expiram (sem punição além da perda do streak
    /// do dia). Expiração preguiçosa: aplicada quando o jogador consulta/conclui missões, o que
    /// dispensa um job à meia-noite e dá o mesmo resultado observável.
    /// </summary>
    private async Task ExpirarVencidasAsync(int playerId, CancellationToken ct)
    {
        var hoje = _relogio.Hoje;
        var vencidas = await _db.Missoes
            .Where(m => m.PlayerId == playerId
                && (m.Status == StatusMissao.Pendente || m.Status == StatusMissao.EmAndamento)
                && m.DataAlvo < hoje)
            .ToListAsync(ct);
        if (vencidas.Count == 0) return;

        foreach (var missao in vencidas)
            missao.Status = StatusMissao.Expirada;
        await _db.SaveChangesAsync(ct);
    }
}
