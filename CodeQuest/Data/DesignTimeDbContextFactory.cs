using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CodeQuest.Data;

/// <summary>
/// Fábrica usada apenas em tempo de design pelo `dotnet ef` (migrations). Constrói o
/// contexto sem subir a aplicação inteira, evitando que o seeder do boot interfira na
/// geração de migrations. Lê a connection string de appsettings.json / variáveis de ambiente.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Carrega o .env (raiz do repo) para que `dotnet ef` leia a mesma connection string da app.
        DotNetEnv.Env.TraversePath().Load();

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conexao = config.GetConnectionString("CodeQuest")
            ?? throw new InvalidOperationException(
                "Connection string 'CodeQuest' não configurada. Defina ConnectionStrings__CodeQuest no .env.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(conexao)
            .Options;

        return new AppDbContext(options);
    }
}
