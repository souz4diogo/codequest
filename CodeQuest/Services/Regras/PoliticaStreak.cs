namespace CodeQuest.Services.Regras;

/// <summary>Resultado do cálculo de streak para uma atividade pontuável.</summary>
/// <param name="NovoStreak">Streak após a atividade.</param>
/// <param name="ContabilizouDia">Se este é o primeiro ponto do dia (a atividade "conta" hoje).</param>
/// <param name="ConsumiuPocao">Se a poção de streak foi gasta para proteger a sequência.</param>
public readonly record struct ResultadoStreak(int NovoStreak, bool ContabilizouDia, bool ConsumiuPocao);

/// <summary>
/// RN05 — Regras de streak. Uma atividade pontuável no dia mantém/incrementa a sequência.
/// Um dia perdido zera o streak, salvo se a poção de streak estiver ativa (protege 1 dia).
/// Função pura: recebe o estado e devolve o novo estado, sem tocar no banco.
/// </summary>
public interface IPoliticaStreak
{
    ResultadoStreak Avaliar(int streakAtual, DateOnly? ultimoDiaAtivo, DateOnly hoje, bool pocaoAtiva);
}

public sealed class PoliticaStreak : IPoliticaStreak
{
    public ResultadoStreak Avaliar(int streakAtual, DateOnly? ultimoDiaAtivo, DateOnly hoje, bool pocaoAtiva)
    {
        // Primeira atividade de todas.
        if (ultimoDiaAtivo is null)
            return new ResultadoStreak(1, ContabilizouDia: true, ConsumiuPocao: false);

        var ultimo = ultimoDiaAtivo.Value;

        // Já pontuou hoje: nada muda.
        if (ultimo == hoje)
            return new ResultadoStreak(streakAtual, ContabilizouDia: false, ConsumiuPocao: false);

        var diasDeDiferenca = hoje.DayNumber - ultimo.DayNumber;

        // Dia consecutivo: incrementa.
        if (diasDeDiferenca == 1)
            return new ResultadoStreak(streakAtual + 1, ContabilizouDia: true, ConsumiuPocao: false);

        // Perdeu exatamente 1 dia e tem poção: protege a sequência (consome a poção).
        if (diasDeDiferenca == 2 && pocaoAtiva)
            return new ResultadoStreak(streakAtual + 1, ContabilizouDia: true, ConsumiuPocao: true);

        // Perdeu 1+ dia sem proteção: zera e recomeça em 1.
        return new ResultadoStreak(1, ContabilizouDia: true, ConsumiuPocao: false);
    }
}
