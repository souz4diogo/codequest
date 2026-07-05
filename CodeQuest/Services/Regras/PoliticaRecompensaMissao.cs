namespace CodeQuest.Services.Regras;

/// <summary>Esforço estimado de uma missão manual, que define a faixa de XP permitida (RN04).</summary>
public enum EsforcoMissao
{
    /// <summary>Rápida (≤ 15 min): 10–20 XP.</summary>
    Rapida,

    /// <summary>Média (≤ 1 h): 30–50 XP.</summary>
    Media,

    /// <summary>Longa (&gt; 1 h): 60–100 XP.</summary>
    Longa
}

/// <summary>Faixa fechada de XP.</summary>
public readonly record struct FaixaXp(int Minimo, int Maximo)
{
    public bool Contem(int xp) => xp >= Minimo && xp <= Maximo;
}

/// <summary>
/// RN04 — Faixas fixas de XP por esforço para missões manuais. Centraliza a validação
/// para que nenhuma outra camada precise conhecer os números mágicos.
/// </summary>
public interface IPoliticaRecompensaMissao
{
    FaixaXp FaixaDe(EsforcoMissao esforco);
    bool XpValido(EsforcoMissao esforco, int xp);
}

public sealed class PoliticaRecompensaMissao : IPoliticaRecompensaMissao
{
    private static readonly IReadOnlyDictionary<EsforcoMissao, FaixaXp> Faixas =
        new Dictionary<EsforcoMissao, FaixaXp>
        {
            [EsforcoMissao.Rapida] = new FaixaXp(10, 20),
            [EsforcoMissao.Media] = new FaixaXp(30, 50),
            [EsforcoMissao.Longa] = new FaixaXp(60, 100),
        };

    public FaixaXp FaixaDe(EsforcoMissao esforco) => Faixas[esforco];

    public bool XpValido(EsforcoMissao esforco, int xp) => Faixas[esforco].Contem(xp);
}
