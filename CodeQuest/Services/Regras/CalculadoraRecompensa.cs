namespace CodeQuest.Services.Regras;

/// <summary>
/// RN02 e RN03 — conversão de XP em gold e multiplicador de streak.
///   - Gold = ⌈XP da atividade / 3⌉ (RN02)
///   - Multiplicador = 1 + min(streak, 5) × 0,10, teto +50% (RN03), aplicado só ao XP.
/// </summary>
public interface ICalculadoraRecompensa
{
    /// <summary>Multiplicador de XP em função do streak atual.</summary>
    double MultiplicadorStreak(int streakDias);

    /// <summary>Aplica o multiplicador de streak ao XP base, arredondando para inteiro.</summary>
    int AplicarMultiplicador(int xpBase, int streakDias);

    /// <summary>Gold ganho a partir do XP efetivamente creditado.</summary>
    int GoldPorXp(int xp);
}

public sealed class CalculadoraRecompensa : ICalculadoraRecompensa
{
    private const int StreakMaximoBonificado = 5;
    private const double BonusPorDia = 0.10;
    private const int DivisorGold = 3;

    public double MultiplicadorStreak(int streakDias)
    {
        var dias = Math.Clamp(streakDias, 0, StreakMaximoBonificado);
        return 1 + dias * BonusPorDia;
    }

    public int AplicarMultiplicador(int xpBase, int streakDias)
    {
        if (xpBase < 0) throw new ArgumentOutOfRangeException(nameof(xpBase));
        return (int)Math.Round(xpBase * MultiplicadorStreak(streakDias), MidpointRounding.AwayFromZero);
    }

    public int GoldPorXp(int xp)
    {
        if (xp < 0) throw new ArgumentOutOfRangeException(nameof(xp));
        return (int)Math.Ceiling((double)xp / DivisorGold);
    }
}
