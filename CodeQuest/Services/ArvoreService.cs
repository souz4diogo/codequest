using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Leitura da árvore de habilidades (RF06) — só projeção; o Status é escrito pelo boss fight
/// (fora do escopo desta rodada) e pelo seeder, nunca calculado aqui.</summary>
public interface IArvoreService
{
    Task<IReadOnlyList<Modulo>> ListarAsync(CancellationToken ct = default);
}

public sealed class ArvoreService : IArvoreService
{
    private readonly AppDbContext _db;

    public ArvoreService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Modulo>> ListarAsync(CancellationToken ct = default) =>
        await _db.Modulos
            .Include(m => m.Topicos)
            .Include(m => m.PreRequisitos)
            .OrderBy(m => m.Ordem)
            .ToListAsync(ct);
}
