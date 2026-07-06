namespace CodeQuest.Models;

/// <summary>Registro de compra na loja (RF20). Guarda o preço pago na época.</summary>
public class CompraLoja
{
    public int Id { get; set; }

    /// <summary>Player que fez a compra (multi-usuário).</summary>
    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int ItemLojaId { get; set; }
    public ItemLoja ItemLoja { get; set; } = null!;

    /// <summary>Preço pago no momento da compra (não muda se o item mudar de preço depois).</summary>
    public int CustoGoldPago { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
