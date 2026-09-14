using CodeQuest.Models;

namespace CodeQuest.Dtos;

/// <summary>Tópico dentro de um módulo, na visão da árvore (RF06).</summary>
public sealed record TopicoResumoDto(int Id, string Nome, int NivelEstimado);

/// <summary>Módulo da árvore de habilidades: status (RF06) + tópicos + pré-requisitos.</summary>
public sealed record ModuloDto(
    int Id,
    string Nome,
    int Ordem,
    StatusModulo Status,
    int? NotaBoss,
    IReadOnlyList<int> RequerModuloIds,
    IReadOnlyList<TopicoResumoDto> Topicos);

public static class MapeamentosArvoreDto
{
    public static TopicoResumoDto ParaResumoDto(this Topico t) => new(t.Id, t.Nome, t.NivelEstimado);

    public static ModuloDto ParaDto(this Modulo m) =>
        new(m.Id, m.Nome, m.Ordem, m.Status, m.NotaBoss,
            m.PreRequisitos.Select(p => p.RequerModuloId).ToList(),
            m.Topicos.Where(t => !t.Arquivado).OrderBy(t => t.Nome).Select(t => t.ParaResumoDto()).ToList());
}
