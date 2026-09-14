using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Quantidade de atividades concluídas num dia (RF24 — gráfico de atividade).</summary>
public sealed record AtividadeDoDia(DateOnly Data, int Quantidade);

/// <summary>Nível médio dos tópicos de um módulo (RF08 — radar de habilidades).</summary>
public sealed record NivelPorModulo(string Modulo, int NivelMedio);

/// <summary>Dados agregados do painel (RF08/RF24): atividade recente e radar de habilidades.
/// Só leitura/projeção — nenhuma regra de jogo vive aqui.</summary>
public interface IDashboardService
{
    Task<IReadOnlyList<AtividadeDoDia>> ObterAtividadeAsync(int dias, CancellationToken ct = default);

    Task<IReadOnlyList<NivelPorModulo>> ObterRadarAsync(CancellationToken ct = default);
}

public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly IRelogio _relogio;
    private readonly IUsuarioAtual _usuario;

    public DashboardService(AppDbContext db, IRelogio relogio, IUsuarioAtual usuario)
    {
        _db = db;
        _relogio = relogio;
        _usuario = usuario;
    }

    public async Task<IReadOnlyList<AtividadeDoDia>> ObterAtividadeAsync(int dias, CancellationToken ct = default)
    {
        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var hoje = _relogio.Hoje;
        var desde = hoje.AddDays(-(dias - 1));
        // Postgres timestamptz só aceita DateTime com Kind=Utc (mesmo cuidado de RelogioSistema).
        var desdeDateTime = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

        // Cada fonte de atividade é player-scoped só onde o schema permite (Missao/SessaoFoco têm
        // PlayerId; Exercicio/Teste/Tópico são globais nesta fase — ver [[codequest-auth]]).
        var missoesConcluidas = await _db.Missoes
            .Where(m => m.PlayerId == playerId && m.ConcluidaEm != null && m.ConcluidaEm >= desdeDateTime)
            .Select(m => DateOnly.FromDateTime(m.ConcluidaEm!.Value))
            .ToListAsync(ct);

        var focosCompletos = await _db.SessoesFoco
            .Where(f => f.PlayerId == playerId && f.MinutosReais >= f.MinutosPlanejados && f.Data >= desdeDateTime)
            .Select(f => DateOnly.FromDateTime(f.Data))
            .ToListAsync(ct);

        var tentativas = await _db.Tentativas
            .Where(t => t.Data >= desdeDateTime)
            .Select(t => DateOnly.FromDateTime(t.Data))
            .ToListAsync(ct);

        var testesCorrigidos = await _db.Testes
            .Where(t => t.Nota != null && t.Data >= desdeDateTime)
            .Select(t => DateOnly.FromDateTime(t.Data))
            .ToListAsync(ct);

        var contagemPorDia = missoesConcluidas.Concat(focosCompletos).Concat(tentativas).Concat(testesCorrigidos)
            .GroupBy(d => d)
            .ToDictionary(g => g.Key, g => g.Count());

        return Enumerable.Range(0, dias)
            .Select(offset => desde.AddDays(offset))
            .Select(dia => new AtividadeDoDia(dia, contagemPorDia.GetValueOrDefault(dia)))
            .ToList();
    }

    public async Task<IReadOnlyList<NivelPorModulo>> ObterRadarAsync(CancellationToken ct = default)
    {
        var medias = await _db.Modulos
            .Where(m => m.Status != StatusModulo.Bloqueado)
            .OrderBy(m => m.Ordem)
            .Select(m => new
            {
                m.Nome,
                Media = m.Topicos.Where(t => !t.Arquivado).Select(t => (int?)t.NivelEstimado).Average(),
            })
            .ToListAsync(ct);

        return medias.Select(m => new NivelPorModulo(m.Nome, (int)Math.Round(m.Media ?? 0))).ToList();
    }
}
