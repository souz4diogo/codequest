namespace CodeQuest.Services.Regras;

/// <summary>
/// RN07 — Nível estimado do tópico (0–100): média ponderada do último teste (peso 3) com as
/// notas dos exercícios dos últimos 30 dias (peso 1 cada). Função pura — o serviço de
/// aplicação seleciona as notas no banco e persiste o resultado em <c>Topico.NivelEstimado</c>
/// (fluxo de correção, Fase 2 do roadmap).
/// </summary>
public interface ICalculadoraNivelTopico
{
    int Calcular(int? notaUltimoTeste, IReadOnlyCollection<int> notasExercicios30Dias);
}

public sealed class CalculadoraNivelTopico : ICalculadoraNivelTopico
{
    private const int PesoTeste = 3;

    public int Calcular(int? notaUltimoTeste, IReadOnlyCollection<int> notasExercicios30Dias)
    {
        var pesoTotal = (notaUltimoTeste is null ? 0 : PesoTeste) + notasExercicios30Dias.Count;
        if (pesoTotal == 0) return 0; // sem dados, o tópico ainda não foi medido

        var soma = (notaUltimoTeste ?? 0) * PesoTeste + notasExercicios30Dias.Sum();
        // Cast seguro: notas são 0–100, então a média também é.
        var media = (int)Math.Round((double)soma / pesoTotal, MidpointRounding.AwayFromZero);
        return Math.Clamp(media, 0, 100);
    }
}
