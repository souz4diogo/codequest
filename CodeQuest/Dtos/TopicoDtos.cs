using CodeQuest.Models;

namespace CodeQuest.Dtos;

/// <summary>Entrada de criação/edição de tópico (RF05).</summary>
public sealed record SalvarTopicoRequest(string Nome, int Prioridade);

/// <summary>Arquiva/desarquiva um tópico (RF05).</summary>
public sealed record ArquivarTopicoRequest(bool Arquivado);

/// <summary>Tópico completo, para telas de gestão (RF05) — diferente do <see cref="TopicoDto"/>
/// enxuto usado no seletor de exercícios.</summary>
public sealed record TopicoGestaoDto(int Id, int ModuloId, string Nome, int NivelEstimado, int Prioridade, bool Arquivado);

public static class MapeamentosTopicoGestaoDto
{
    public static TopicoGestaoDto ParaGestaoDto(this Topico t) =>
        new(t.Id, t.ModuloId, t.Nome, t.NivelEstimado, t.Prioridade, t.Arquivado);
}
