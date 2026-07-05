namespace CodeQuest.Models;

/// <summary>Item vendável na loja (RF20). Seed com os itens do plano.</summary>
public class ItemLoja
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int CustoGold { get; set; }
    public bool Ativo { get; set; } = true;

    // Navegação
    public ICollection<CompraLoja> Compras { get; set; } = new List<CompraLoja>();
}
