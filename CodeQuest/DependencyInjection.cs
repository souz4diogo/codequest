using System.Text;
using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Loja;
using CodeQuest.Services.Regras;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        // Autenticação (cookie do Blazor + JWT da API React) e serviços de identidade
        services.AddAutenticacaoCodeQuest(config);
        services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
        services.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();

        // Seed
        services.AddScoped<ISeeder, DatabaseSeeder>();

        return services;
    }

    /// <summary>
    /// Dois esquemas coexistem durante a migração:
    /// <list type="bullet">
    ///   <item><b>Cookie</b> (padrão) — usado pelas telas Blazor ainda vivas.</item>
    ///   <item><b>JWT Bearer</b> — usado pela API que a SPA React consome (endpoints <c>[Authorize(Bearer)]</c>).</item>
    /// </list>
    /// Quando o Blazor sair, o cookie e o <c>CascadingAuthenticationState</c> saem com ele.
    /// </summary>
    private static IServiceCollection AddAutenticacaoCodeQuest(this IServiceCollection services, IConfiguration config)
    {
        var jwt = LerOpcoesJwt(config);
        services.AddSingleton(jwt);
        services.AddSingleton<IGeradorTokenJwt, GeradorTokenJwt>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(opt =>
            {
                opt.Cookie.Name = "CodeQuest.Auth";
                opt.LoginPath = "/login";
                opt.LogoutPath = "/logout";
                opt.AccessDeniedPath = "/login";
                opt.ExpireTimeSpan = TimeSpan.FromDays(7);
                opt.SlidingExpiration = true;
                opt.Cookie.HttpOnly = true;
                opt.Cookie.SameSite = SameSiteMode.Lax;
            })
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
        services.AddCascadingAuthenticationState();

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
