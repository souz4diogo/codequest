using CodeQuest.Dtos;
using CodeQuest.Services;
using CodeQuest.Services.Loja;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>
/// Loja do jogador logado (RF20). Toda a autoridade sobre gold é do backend (RN09): o
/// <see cref="ILojaService"/> debita e aplica os efeitos; o controller só orquestra e mapeia.
/// </summary>
[ApiController]
[Route("api/loja")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class LojaController : ControllerBase
{
    private readonly ILojaService _loja;
    private readonly IGameService _game;

    public LojaController(ILojaService loja, IGameService game)
    {
        _loja = loja;
        _game = game;
    }

    /// <summary>Itens ativos da loja, ordenados por preço.</summary>
    [HttpGet("itens")]
    public async Task<ActionResult<IReadOnlyList<ItemLojaDto>>> Itens(CancellationToken ct)
    {
        var itens = await _loja.ListarAtivosAsync(ct);
        return Ok(itens.Select(i => i.ParaDto()).ToList());
    }

    /// <summary>Compra um item. Falha (400) se o item não existir ou faltar gold.</summary>
    [HttpPost("comprar/{itemId:int}")]
    public async Task<ActionResult<CompraDto>> Comprar(int itemId, CancellationToken ct)
    {
        var resultado = await _loja.ComprarAsync(itemId, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } compra)
            return BadRequest(new { erro = resultado.Erro });

        // Relê o player para devolver o gold já debitado — o front atualiza a UI sem recarregar tudo.
        var player = await _game.ObterPlayerAsync(ct);
        return new CompraDto(compra.ItemLojaId, compra.CustoGoldPago, player.Gold);
    }
}
