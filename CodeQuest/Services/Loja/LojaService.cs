using CodeQuest.Auth;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Services.Loja;

/// <summary>
/// Loja (RF20). Debita gold, registra a compra com o preço da época e aplica os efeitos
/// do item. Toda a autoridade sobre gold é do backend (RN09).
/// </summary>
public interface ILojaService
{
    Task<IReadOnlyList<ItemLoja>> ListarAtivosAsync(CancellationToken ct = default);

    /// <summary>Compra um item. Falha (sem exceção) se o item não existir ou faltar gold.</summary>
    Task<Resultado<CompraLoja>> ComprarAsync(int itemId, CancellationToken ct = default);
}

public sealed class LojaService : ILojaService
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<IEfeitoItemLoja> _efeitos;
    private readonly IUsuarioAtual _usuario;

    public LojaService(AppDbContext db, IEnumerable<IEfeitoItemLoja> efeitos, IUsuarioAtual usuario)
    {
        _db = db;
        _efeitos = efeitos;
        _usuario = usuario;
    }

    public async Task<IReadOnlyList<ItemLoja>> ListarAtivosAsync(CancellationToken ct = default)
        => await _db.ItensLoja.Where(i => i.Ativo).OrderBy(i => i.CustoGold).ToListAsync(ct);

    public async Task<Resultado<CompraLoja>> ComprarAsync(int itemId, CancellationToken ct = default)
    {
        var item = await _db.ItensLoja.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null || !item.Ativo)
            return Resultado.Falha<CompraLoja>("Item indisponível.");

        var playerId = await _usuario.ObterPlayerIdAsync(ct);
        var player = await _db.Players.FirstOrDefaultAsync(p => p.Id == playerId, ct)
                     ?? throw new InvalidOperationException("Player do usuário logado não encontrado.");

        if (player.Gold < item.CustoGold)
            return Resultado.Falha<CompraLoja>($"Gold insuficiente: precisa de {item.CustoGold}, tem {player.Gold}.");

        player.Gold -= item.CustoGold;

        // Aplica efeitos registrados para este item (Strategy/OCP).
        foreach (var efeito in _efeitos.Where(e => e.AplicaSe(item)))
            efeito.Aplicar(player, item);

        var compra = new CompraLoja
        {
            PlayerId = player.Id,
            ItemLojaId = item.Id,
            CustoGoldPago = item.CustoGold,
        };
        _db.ComprasLoja.Add(compra);

        await _db.SaveChangesAsync(ct);
        return Resultado.Ok(compra);
    }
}
