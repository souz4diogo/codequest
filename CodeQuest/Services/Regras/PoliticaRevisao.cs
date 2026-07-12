namespace CodeQuest.Services.Regras;

/// <summary>Agendamento resultante de uma reprovação: ciclo da revisão e a data em que ela cai.</summary>
public readonly record struct ProximaRevisao(int Ciclo, DateOnly AgendadaPara);

/// <summary>
/// RN06 — Exercício aprovado com nota ≥ 70; reprovado entra na fila de repetição espaçada
/// em ciclos de +1, +3 e +7 dias (RF16). Função pura: recebe o estado e devolve o
/// agendamento — quem grava a <c>Revisao</c> é o serviço de aplicação (fluxo de tentativas,
/// Fase 2 do roadmap).
/// </summary>
public interface IPoliticaRevisao
{
    /// <summary>Se a nota aprova o exercício (RN06).</summary>
    bool Aprovado(int nota);

    /// <summary>
    /// Próxima revisão após uma reprovação. <paramref name="cicloAtual"/> = 0 quando ainda
    /// não houve revisão. Devolve null quando os 3 ciclos já se esgotaram.
    /// </summary>
    ProximaRevisao? AposReprovar(int cicloAtual, DateOnly hoje);
}

public sealed class PoliticaRevisao : IPoliticaRevisao
{
    public const int NotaMinima = 70;

    /// <summary>Dias de espera de cada ciclo da repetição espaçada: 1, 3 e 7 (RF16).</summary>
    private static readonly int[] DiasPorCiclo = [1, 3, 7];

    public bool Aprovado(int nota)
    {
        if (nota is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(nota));
        return nota >= NotaMinima;
    }

    public ProximaRevisao? AposReprovar(int cicloAtual, DateOnly hoje)
    {
        if (cicloAtual < 0) throw new ArgumentOutOfRangeException(nameof(cicloAtual));
        if (cicloAtual >= DiasPorCiclo.Length) return null; // 3 ciclos cumpridos: sai da fila

        var ciclo = cicloAtual + 1;
        return new ProximaRevisao(ciclo, hoje.AddDays(DiasPorCiclo[ciclo - 1]));
    }
}
