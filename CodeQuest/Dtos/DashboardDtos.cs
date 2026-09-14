using CodeQuest.Services;

namespace CodeQuest.Dtos;

/// <summary>Um dia do gráfico de atividade (RF24).</summary>
public sealed record AtividadeDoDiaDto(DateOnly Data, int Quantidade);

/// <summary>Um eixo do radar de habilidades (RF08).</summary>
public sealed record NivelPorModuloDto(string Modulo, int NivelMedio);

public static class MapeamentosDashboardDto
{
    public static AtividadeDoDiaDto ParaDto(this AtividadeDoDia a) => new(a.Data, a.Quantidade);

    public static NivelPorModuloDto ParaDto(this NivelPorModulo n) => new(n.Modulo, n.NivelMedio);
}
