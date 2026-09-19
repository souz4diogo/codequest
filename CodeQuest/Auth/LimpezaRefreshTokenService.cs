using CodeQuest.Common;
using CodeQuest.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Auth;

/// <summary>
/// Faxina periódica da tabela RefreshTokens: sem isso, toda rotação/logout deixa uma linha morta
/// pra trás e a tabela só cresce. Roda uma vez no boot e depois a cada <see cref="Intervalo"/> —
/// hygiene de dados, não regra de negócio, então um job simples (sem coordenação entre players,
/// diferente da geração de missões diárias) é a ferramenta certa aqui.
/// </summary>
public sealed class LimpezaRefreshTokenService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LimpezaRefreshTokenService> _logger;

    public LimpezaRefreshTokenService(IServiceScopeFactory scopeFactory, ILogger<LimpezaRefreshTokenService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var relogio = scope.ServiceProvider.GetRequiredService<IRelogio>();

                var apagados = await LimparAsync(db, relogio, stoppingToken);
                if (apagados > 0)
                    _logger.LogInformation("Limpeza de refresh tokens: {Apagados} linha(s) removida(s).", apagados);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Uma falha aqui (ex.: banco fora do ar num restart) não pode derrubar a API —
                // só tenta de novo no próximo ciclo.
                _logger.LogError(ex, "Falha na limpeza periódica de refresh tokens.");
            }

            try
            {
                await Task.Delay(Intervalo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Apaga refresh tokens expirados ou já revogados. Extraído à parte pra ser testável sem
    /// BackgroundService. RemoveRange (não ExecuteDelete) de propósito: a tabela é pequena pra
    /// uso pessoal e isso mantém o método testável com EF Core InMemory, igual ao resto do projeto.
    /// </summary>
    public static async Task<int> LimparAsync(AppDbContext db, IRelogio relogio, CancellationToken ct = default)
    {
        var mortos = await db.RefreshTokens
            .Where(r => r.RevogadoEm != null || r.ExpiraEm <= relogio.Agora)
            .ToListAsync(ct);

        db.RefreshTokens.RemoveRange(mortos);
        await db.SaveChangesAsync(ct);
        return mortos.Count;
    }
}
