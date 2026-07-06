using CodeQuest.Models;
using CodeQuest.Services.Loja;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Data;

/// <summary>Popula o banco com os dados globais iniciais (módulos, tópicos, loja). Item 0.3 do backlog.
/// O Player não é semeado: cada um nasce no registro do seu usuário.</summary>
public interface ISeeder
{
    Task ExecutarAsync(CancellationToken ct = default);
}

/// <summary>
/// Seed idempotente: só insere o que ainda não existe, então pode rodar a cada boot com
/// segurança. Aplica as migrations pendentes antes de semear.
/// </summary>
public sealed class DatabaseSeeder : ISeeder
{
    private readonly AppDbContext _db;

    public DatabaseSeeder(AppDbContext db) => _db = db;

    public async Task ExecutarAsync(CancellationToken ct = default)
    {
        await _db.Database.MigrateAsync(ct);

        await SemearArvoreAsync(ct);
        await SemearLojaAsync(ct);

        await _db.SaveChangesAsync(ct);
    }

    private async Task SemearArvoreAsync(CancellationToken ct)
    {
        if (await _db.Modulos.AnyAsync(ct)) return;

        // 10 módulos da trilha de C#. O primeiro nasce Liberado; os demais, Bloqueados.
        var modulos = new List<Modulo>
        {
            new() { Ordem = 1, Nome = "Fundamentos C#", Status = StatusModulo.Liberado },
            new() { Ordem = 2, Nome = "Orientação a Objetos", Status = StatusModulo.Bloqueado },
            new() { Ordem = 3, Nome = "Coleções e Genéricos", Status = StatusModulo.Bloqueado },
            new() { Ordem = 4, Nome = "LINQ", Status = StatusModulo.Bloqueado },
            new() { Ordem = 5, Nome = "Tratamento de Erros e Debugging", Status = StatusModulo.Bloqueado },
            new() { Ordem = 6, Nome = "Async e Concorrência", Status = StatusModulo.Bloqueado },
            new() { Ordem = 7, Nome = "Acesso a Dados (EF Core)", Status = StatusModulo.Bloqueado },
            new() { Ordem = 8, Nome = "APIs Web (ASP.NET Core)", Status = StatusModulo.Bloqueado },
            new() { Ordem = 9, Nome = "Testes Automatizados", Status = StatusModulo.Bloqueado },
            new() { Ordem = 10, Nome = "Arquitetura e SOLID", Status = StatusModulo.Bloqueado },
        };
        _db.Modulos.AddRange(modulos);
        await _db.SaveChangesAsync(ct); // gera os Ids para os pré-requisitos e tópicos

        Modulo M(int ordem) => modulos.First(m => m.Ordem == ordem);

        // Pré-requisitos (RF06). Cada par: módulo → módulo exigido.
        var prereqs = new (int Modulo, int Requer)[]
        {
            (2, 1), (3, 2), (4, 3), (5, 2), (6, 3),
            (7, 3), (8, 7), (9, 2), (10, 8), (10, 9),
        };
        _db.ModuloPrereqs.AddRange(prereqs.Select(p => new ModuloPrereq
        {
            ModuloId = M(p.Modulo).Id,
            RequerModuloId = M(p.Requer).Id,
        }));

        // Tópicos iniciais dos primeiros módulos.
        var topicosPorModulo = new Dictionary<int, string[]>
        {
            [1] = ["Tipos e variáveis", "Controle de fluxo", "Métodos", "Strings"],
            [2] = ["Classes e objetos", "Herança", "Interfaces", "Polimorfismo"],
            [3] = ["List e arrays", "Dictionary", "Genéricos"],
            [4] = ["Consultas LINQ", "Lambdas", "Métodos de extensão"],
        };
        foreach (var (ordem, nomes) in topicosPorModulo)
            _db.Topicos.AddRange(nomes.Select(nome => new Topico
            {
                ModuloId = M(ordem).Id,
                Nome = nome,
            }));
    }

    private async Task SemearLojaAsync(CancellationToken ct)
    {
        if (await _db.ItensLoja.AnyAsync(ct)) return;

        _db.ItensLoja.AddRange(
            new ItemLoja { Nome = ItensLojaConhecidos.PocaoDeStreak, CustoGold = 50 },
            new ItemLoja { Nome = "Dica Extra", CustoGold = 20 },
            new ItemLoja { Nome = "Tema Escuro", CustoGold = 30 },
            new ItemLoja { Nome = "Boost XP 2x (1h)", CustoGold = 80 },
            new ItemLoja { Nome = "Skin Pixel Art", CustoGold = 100 });
    }
}
