namespace CodeQuest.Services.Regras;

/// <summary>
/// RN01 — Progressão de nível a partir do XP acumulado: alcança o nível N quando
/// XPacumulado ≥ 50 × N². Âncoras do plano (seção 2): nível 2 = 200 XP,
/// nível 5 = 1.250, nível 10 = 5.000.
///   - XP para alcançar o nível N = 50 × N² (nível 1 começa em 0)
///   - Nível(xp) = max(1, ⌊√(xp / 50)⌋)
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
        return Math.Max(1, (int)Math.Floor(Math.Sqrt((double)xpTotal / Fator)));
    }

    public int XpParaNivel(int nivel)
    {
        if (nivel < 1) throw new ArgumentOutOfRangeException(nameof(nivel));
        return nivel == 1 ? 0 : Fator * nivel * nivel;
    }

    public int XpFaltandoParaProximoNivel(int xpTotal)
    {
        var proximo = CalcularNivel(xpTotal) + 1;
        return Math.Max(0, XpParaNivel(proximo) - xpTotal);
    }
}
