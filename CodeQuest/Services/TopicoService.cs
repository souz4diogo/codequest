using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services;

/// <summary>Dados editáveis de um tópico (RF05).</summary>
public readonly record struct DadosTopico(string Nome, int Prioridade);

/// <summary>CRUD de tópicos de estudo (RF05): criar, editar, priorizar e arquivar.</summary>
public interface ITopicoService
{
    /// <summary>Todos os tópicos do módulo, incluindo arquivados (tela de gestão, RF05).</summary>
    Task<IReadOnlyList<Topico>> ListarAsync(int moduloId, CancellationToken ct = default);

    Task<Resultado<Topico>> CriarAsync(int moduloId, DadosTopico dados, CancellationToken ct = default);

    Task<Resultado<Topico>> EditarAsync(int moduloId, int topicoId, DadosTopico dados, CancellationToken ct = default);

    Task<Resultado<Topico>> ArquivarAsync(int moduloId, int topicoId, bool arquivado, CancellationToken ct = default);
}

public sealed class TopicoService : ITopicoService
{
    private const int PrioridadeMinima = 1;
    private const int PrioridadeMaxima = 3;

    private readonly AppDbContext _db;

    public TopicoService(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<Topico>> ListarAsync(int moduloId, CancellationToken ct = default) =>
        await _db.Topicos.Where(t => t.ModuloId == moduloId).OrderBy(t => t.Nome).ToListAsync(ct);

    public async Task<Resultado<Topico>> CriarAsync(int moduloId, DadosTopico dados, CancellationToken ct = default)
    {
        var erro = Validar(dados);
        if (erro is not null) return Resultado.Falha<Topico>(erro);

        var modulo = await _db.Modulos.FirstOrDefaultAsync(m => m.Id == moduloId, ct);
        if (modulo is null)
            return Resultado.Falha<Topico>("Módulo não encontrado.");

        var topico = new Topico { ModuloId = modulo.Id, Nome = dados.Nome.Trim(), Prioridade = dados.Prioridade };
        _db.Topicos.Add(topico);
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(topico);
    }

    public async Task<Resultado<Topico>> EditarAsync(int moduloId, int topicoId, DadosTopico dados, CancellationToken ct = default)
    {
        var erro = Validar(dados);
        if (erro is not null) return Resultado.Falha<Topico>(erro);

        var topico = await _db.Topicos.FirstOrDefaultAsync(t => t.Id == topicoId && t.ModuloId == moduloId, ct);
        if (topico is null)
            return Resultado.Falha<Topico>("Tópico não encontrado.");

        topico.Nome = dados.Nome.Trim();
        topico.Prioridade = dados.Prioridade;
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(topico);
    }

    public async Task<Resultado<Topico>> ArquivarAsync(int moduloId, int topicoId, bool arquivado, CancellationToken ct = default)
    {
        var topico = await _db.Topicos.FirstOrDefaultAsync(t => t.Id == topicoId && t.ModuloId == moduloId, ct);
        if (topico is null)
            return Resultado.Falha<Topico>("Tópico não encontrado.");

        topico.Arquivado = arquivado;
        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(topico);
    }

    private static string? Validar(DadosTopico dados)
    {
        if (string.IsNullOrWhiteSpace(dados.Nome))
            return "Nome é obrigatório.";
        if (dados.Prioridade is < PrioridadeMinima or > PrioridadeMaxima)
            return $"Prioridade deve estar entre {PrioridadeMinima} e {PrioridadeMaxima}.";
        return null;
    }
}
