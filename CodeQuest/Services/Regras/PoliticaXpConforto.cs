using CodeQuest.Models;

namespace CodeQuest.Services.Regras;

/// <summary>
/// RN11 — XP decrescente por conforto (motor-ia §7): o XP do exercício é relativo ao nível do
/// jogador no tópico. Nível ≥ 40: easy paga 50%; nível ≥ 60: easy paga 10% e medium 60% —
/// só hard/expert seguem pagando cheio. Farmar fácil vira visivelmente ruim, sem proibir nada.
/// </summary>
public interface IPoliticaXpConforto
{
    /// <summary>Fator (0–1] aplicado ao XP da tabela conforme o nível no tópico.</summary>
    double Fator(Dificuldade dificuldade, int nivelTopico);

    /// <summary>XP da tabela já com o desconto de conforto aplicado.</summary>
    int Ajustar(int xpBase, Dificuldade dificuldade, int nivelTopico);
}

public sealed class PoliticaXpConforto : IPoliticaXpConforto
{
    public const int NivelConfortoEasy = 40;   // easy começa a pagar menos
    public const int NivelConfortoMedio = 60;  // easy quase zera; medium reduz

    public double Fator(Dificuldade dificuldade, int nivelTopico) => dificuldade switch
    {
        Dificuldade.Easy when nivelTopico >= NivelConfortoMedio => 0.10,
        Dificuldade.Easy when nivelTopico >= NivelConfortoEasy => 0.50,
        Dificuldade.Medium when nivelTopico >= NivelConfortoMedio => 0.60,
        _ => 1.0, // hard/expert sempre pagam cheio
    };

    public int Ajustar(int xpBase, Dificuldade dificuldade, int nivelTopico)
    {
        if (xpBase < 0) throw new ArgumentOutOfRangeException(nameof(xpBase));
        return (int)Math.Round(xpBase * Fator(dificuldade, nivelTopico), MidpointRounding.AwayFromZero);
    }
}
