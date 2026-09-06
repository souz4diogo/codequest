using System.Text;
using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Ai;
using CodeQuest.Services.Loja;
using CodeQuest.Services.Regras;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
        services.AddHttpContextAccessor();

        // Regras de negócio (puras, stateless → singleton) — RN01–RN10 centralizadas aqui
        services.AddSingleton<IReguladorNivel, ReguladorNivel>();               // RN01
        services.AddSingleton<ICalculadoraRecompensa, CalculadoraRecompensa>(); // RN02/RN03
        services.AddSingleton<IPoliticaRecompensaMissao, PoliticaRecompensaMissao>(); // RN04
        services.AddSingleton<IPoliticaStreak, PoliticaStreak>();               // RN05
        services.AddSingleton<IPoliticaRevisao, PoliticaRevisao>();             // RN06
        services.AddSingleton<ICalculadoraNivelTopico, CalculadoraNivelTopico>(); // RN07
        services.AddSingleton<IPoliticaArvore, PoliticaArvore>();               // RN08
        services.AddSingleton<ITabelaRecompensas, TabelaRecompensas>();         // RN09

        // Efeitos de itens da loja (Strategy/OCP) — registre novos itens com efeito aqui.
        services.AddScoped<IEfeitoItemLoja, EfeitoPocaoStreak>();

        // Serviços de aplicação (usam o DbContext scoped)
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<ILojaService, LojaService>();
        services.AddScoped<IMissaoService, MissaoService>();
        services.AddScoped<IExercicioService, ExercicioService>();

        // IA (Gemini) — chave em Gemini:ApiKey (user secrets/env, RNF02). Sem timeout no HttpClient
        // de propósito: o dele cobriria a leitura do corpo inteiro e matava geração longa no meio.
        // Quem corta é o timeout de INATIVIDADE do GeminiClient, que lê a resposta em stream (RNF04);
        // o retry também vive lá. O app sobe e funciona sem a chave (modo degradado, RNF07).
        services.AddHttpClient<IGeminiClient, GeminiClient>(http => http.Timeout = Timeout.InfiniteTimeSpan);

        // Autenticação (JWT da API React) e serviços de identidade
        services.AddAutenticacaoCodeQuest(config);
        services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        services.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();

        // Seed
        services.AddScoped<ISeeder, DatabaseSeeder>();

        return services;
    }

    /// <summary>
    /// Autenticação por <b>JWT Bearer</b>: a SPA React consome a API enviando o token no header
    /// <c>Authorization</c> (endpoints <c>[Authorize]</c>). Não há sessão no servidor.
    /// </summary>
    private static IServiceCollection AddAutenticacaoCodeQuest(this IServiceCollection services, IConfiguration config)
    {
        var jwt = LerOpcoesJwt(config);
        services.AddSingleton(jwt);
        services.AddSingleton<IGeradorTokenJwt, GeradorTokenJwt>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                // Claims customizados (ex.: codequest:player_id) chegam sem remapeamento.
                opt.MapInboundClaims = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>Lê as opções do JWT: Issuer/Audience/expiração do appsettings e o segredo do ambiente (Jwt__Secret).</summary>
    private static OpcoesJwt LerOpcoesJwt(IConfiguration config)
    {
        var secret = config["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "Segredo JWT não configurado. Defina 'Jwt__Secret' no ambiente/.env (>= 32 caracteres).");

        var issuer = config["Jwt:Issuer"] ?? "CodeQuest";
        var audience = config["Jwt:Audience"] ?? "CodeQuest";
        var expira = int.TryParse(config["Jwt:ExpiraEmMinutos"], out var minutos) ? minutos : 60;

        return new OpcoesJwt(secret, issuer, audience, expira);
    }
}
