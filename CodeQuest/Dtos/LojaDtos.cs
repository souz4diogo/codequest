using CodeQuest.Models;

namespace CodeQuest.Dtos;

/// <summary>Item da loja exposto ao front (RF20).</summary>
public sealed record ItemLojaDto(int Id, string Nome, int CustoGold);

/// <summary>Comprovante de compra devolvido ao front, com o gold restante do jogador já atualizado.</summary>
public sealed record CompraDto(int ItemLojaId, int CustoGoldPago, int GoldRestante);

/// <summary>Mapeamentos da loja → DTO.</summary>
public static class MapeamentosLojaDto
{
    public static ItemLojaDto ParaDto(this ItemLoja i) => new(i.Id, i.Nome, i.CustoGold);
}
