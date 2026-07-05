namespace CodeQuest.Services.Regras;

/// <summary>
/// RN01 — Progressão de nível a partir do XP acumulado.
/// O documento define o limiar como "XP acumulado ≥ 50 × N²". Interpretamos esse valor
/// como o XP necessário para ALCANÇAR o nível N+1 (curva clássica de gamificação), de forma
/// que o jogador comece no nível 1 com 0 XP:
///   - XP para chegar ao nível N = 50 × (N-1)²   →  nível 1 = 0, nível 2 = 50, nível 3 = 200…
///   - Nível(xp) = 1 + ⌊√(xp / 50)⌋
/// </summary>
public interface IReguladorNivel
{
    /// <summary>Nível correspondente a um XP total.</summary>
    int CalcularNivel(int xpTotal);

    /// <summary>XP mínimo para alcançar um dado nível.</summary>
    int XpParaNivel(int nivel);

    /// <summary>XP que ainda falta para o próximo nível.</summary>
    int XpFaltandoParaProximoNivel(int xpTotal);
}

public sealed class ReguladorNivel : IReguladorNivel
{
    private const int Fator = 50;

    public int CalcularNivel(int xpTotal)
    {
        if (xpTotal < 0) throw new ArgumentOutOfRangeException(nameof(xpTotal));
        return 1 + (int)Math.Floor(Math.Sqrt((double)xpTotal / Fator));
    }

    public int XpParaNivel(int nivel)
    {
        if (nivel < 1) throw new ArgumentOutOfRangeException(nameof(nivel));
        var n = nivel - 1;
        return Fator * n * n;
    }

    public int XpFaltandoParaProximoNivel(int xpTotal)
    {
        var proximo = CalcularNivel(xpTotal) + 1;
        return Math.Max(0, XpParaNivel(proximo) - xpTotal);
    }
}
