using CodeQuest.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Tests.Suporte;

/// <summary>Cria um AppDbContext InMemory isolado por teste (banco novo a cada chamada).</summary>
public static class DbContextFactory
{
    public static AppDbContext Criar()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
