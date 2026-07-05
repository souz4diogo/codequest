using CodeQuest.Models;

namespace CodeQuest.Services.Loja;

/// <summary>
/// Efeito aplicado quando um item da loja é comprado (Strategy). Permite adicionar novos
/// itens com comportamento sem alterar o LojaService (OCP): basta registrar mais uma
/// implementação no contêiner de DI.
/// </summary>
public interface IEfeitoItemLoja
{
    /// <summary>Se este efeito se aplica ao item comprado.</summary>
    bool AplicaSe(ItemLoja item);

    /// <summary>Aplica o efeito ao jogador (ex.: ativar poção de streak).</summary>
    void Aplicar(Player player, ItemLoja item);
}

/// <summary>Nomes canônicos dos itens de loja com efeito conhecido, evitando strings soltas.</summary>
public static class ItensLojaConhecidos
{
    public const string PocaoDeStreak = "Poção de Streak";
}

/// <summary>RF21 — a poção de streak, ao ser comprada, fica ativa para proteger 1 dia de streak.</summary>
public sealed class EfeitoPocaoStreak : IEfeitoItemLoja
{
    public bool AplicaSe(ItemLoja item) =>
        string.Equals(item.Nome, ItensLojaConhecidos.PocaoDeStreak, StringComparison.OrdinalIgnoreCase);

    public void Aplicar(Player player, ItemLoja item) => player.PocaoStreakAtiva = true;
}
