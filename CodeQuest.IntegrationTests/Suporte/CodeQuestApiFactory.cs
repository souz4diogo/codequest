using CodeQuest.Data;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Services.Ai;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Xunit;

namespace CodeQuest.IntegrationTests;

/// <summary>
/// Sobe a API inteira (Program.cs real) contra um Postgres descartável (Testcontainers) —
/// valida as migrations, os check constraints do banco e a autenticação JWT de ponta a ponta.
/// A IA (Gemini) é substituída por <see cref="GeminiClientFake"/>: os testes nunca dependem de
/// rede/chave de API para exercitar os endpoints que geram conteúdo.
/// </summary>
public sealed class CodeQuestApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    static CodeQuestApiFactory()
    {
        // WebApplicationBuilder liga um FileSystemWatcher no appsettings.json (reloadOnChange).
        // Em checkouts montados via UNC (\\wsl.localhost\...), o 9P do WSL2 não suporta a
        // notificação de mudança de diretório do Windows e o watcher trava o boot indefinidamente.
        // Precisa ser lido ANTES de WebApplication.CreateBuilder, então fica no construtor
        // estático (garantido rodar antes de qualquer host ser construído).
        Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");

        // Placeholder deliberadamente inválido: se InitializeAsync não rodar antes do primeiro
        // host ser construído (não deveria acontecer — xUnit sempre espera a collection fixture
        // primeiro), a app falha rápido e alto em vez de silenciosamente conectar em outro lugar.
        // Program.cs carrega o .env com clobberExistingVars:false, então esses valores (setados
        // ANTES do host subir) sobrevivem ao DotNetEnv.Load() — sem isso, o .env do repo
        // (ConnectionStrings__CodeQuest apontando pro Postgres de DESENVOLVIMENTO real) vencia e
        // os testes escreviam dados de teste no banco real (incidente descoberto em 2026-09-14).
        Environment.SetEnvironmentVariable("ConnectionStrings__CodeQuest", "Host=placeholder-nao-inicializado;Port=1;Database=x;Username=x;Password=x");
        Environment.SetEnvironmentVariable("Jwt__Secret", "segredo-de-teste-para-integracao-com-32-chars+");
    }

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("codequest_test")
        .WithUsername("codequest")
        .WithPassword("codequest")
        .Build();

    public GeminiClientFake Gemini { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CodeQuest"] = _postgres.GetConnectionString(),
                ["Jwt:Secret"] = "segredo-de-teste-para-integracao-com-32-chars+",
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IGeminiClient>();
            services.AddSingleton<IGeminiClient>(Gemini);
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        // A fonte de verdade: variável de ambiente real, lida por AddEnvironmentVariables()
        // durante WebApplication.CreateBuilder — o ConfigureAppConfiguration acima é só um
        // reforço, não confiável sozinho (ver comentário no construtor estático).
        Environment.SetEnvironmentVariable("ConnectionStrings__CodeQuest", _postgres.GetConnectionString());
    }

    public new async Task DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }

    /// <summary>Escopo novo com o AppDbContext, para arranjar dados direto no banco (sem passar pela API).</summary>
    public AppDbContext CriarDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}
