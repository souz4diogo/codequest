using CodeQuest.Models;

namespace CodeQuest.Services.Regras;

/// <summary>
/// RN12 — Dificuldade recomendada automática (motor-ia §7): mantém a taxa de acerto recente
/// do tópico na zona onde se aprende de verdade (60–85%). Acima de 85% sobe a dificuldade;
/// abaixo de 50% desce sem drama. Sem histórico, começa em Easy (construir confiança).
/// </summary>
public interface IPoliticaDificuldadeAdaptativa
{
    Dificuldade Recomendar(Dificuldade atual, double taxaAcerto, int tentativasRecentes);
}

public sealed class PoliticaDificuldadeAdaptativa : IPoliticaDificuldadeAdaptativa
{
    public const double AcertoParaSubir = 0.85;
    public const double AcertoParaDescer = 0.50;

    public Dificuldade Recomendar(Dificuldade atual, double taxaAcerto, int tentativasRecentes)
    {
        if (tentativasRecentes < 0) throw new ArgumentOutOfRangeException(nameof(tentativasRecentes));
        if (tentativasRecentes == 0) return Dificuldade.Easy;

        if (taxaAcerto > AcertoParaSubir) return Subir(atual);
        if (taxaAcerto < AcertoParaDescer) return Descer(atual);
        return atual;
    }

    private static Dificuldade Subir(Dificuldade d) =>
        d == Dificuldade.Expert ? d : (Dificuldade)((int)d + 1);

    private static Dificuldade Descer(Dificuldade d) =>
        d == Dificuldade.Easy ? d : (Dificuldade)((int)d - 1);
}
