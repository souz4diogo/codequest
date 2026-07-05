using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Services;
using CodeQuest.Services.Loja;
using CodeQuest.Services.Regras;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest;

/// <summary>
/// Composição da aplicação (Composition Root). Concentra o registro das dependências para
/// manter o Program.cs enxuto e deixar explícito o mapeamento abstração → implementação.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddCodeQuest(this IServiceCollection services, IConfiguration config)
    {
        var conexao = config.GetConnectionString("CodeQuest")
            ?? throw new InvalidOperationException(
                "Connection string 'CodeQuest' não configurada (appsettings ou user-secrets).");
        services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(conexao));

        // Infraestrutura
        services.AddSingleton<IRelogio, RelogioSistema>();

        // Regras de negócio (puras, stateless → singleton)
        services.AddSingleton<IReguladorNivel, ReguladorNivel>();
        services.AddSingleton<ICalculadoraRecompensa, CalculadoraRecompensa>();
        services.AddSingleton<IPoliticaStreak, PoliticaStreak>();
        services.AddSingleton<IPoliticaRecompensaMissao, PoliticaRecompensaMissao>();

        // Efeitos de itens da loja (Strategy/OCP) — registre novos itens com efeito aqui.
        services.AddScoped<IEfeitoItemLoja, EfeitoPocaoStreak>();

        // Serviços de aplicação (usam o DbContext scoped)
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILojaService, LojaService>();
        services.AddScoped<IMissaoService, MissaoService>();

        // Seed
        services.AddScoped<ISeeder, DatabaseSeeder>();

        return services;
    }
}
