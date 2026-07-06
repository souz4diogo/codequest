using CodeQuest.Dtos;
using CodeQuest.Services;
using CodeQuest.Services.Regras;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>
/// Estado do jogador logado. Protegido por JWT (o <see cref="IGameService"/> resolve o player
/// a partir do token via <c>IUsuarioAtual</c> — o controller não recebe nem confia em id vindo do cliente).
/// </summary>
[ApiController]
[Route("api/player")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class PlayerController : ControllerBase
{
    private readonly IGameService _game;
    private readonly IReguladorNivel _nivel;

    public PlayerController(IGameService game, IReguladorNivel nivel)
    {
        _game = game;
        _nivel = nivel;
    }

    [HttpGet]
    public async Task<ActionResult<PlayerDto>> Obter(CancellationToken ct)
    {
        var player = await _game.ObterPlayerAsync(ct);
        return player.ParaDto(_nivel);
    }

    /// <summary>Endpoint de teste (dev): credita XP direto, como os botões "+XP" do Dashboard antigo.</summary>
    [HttpPost("xp")]
    public async Task<ActionResult<ResultadoXpDto>> AdicionarXp(AdicionarXpRequest req, CancellationToken ct)
    {
        if (req.XpBase < 0)
            return BadRequest(new { erro = "XP não pode ser negativo." });

        var resultado = await _game.AdicionarXpAsync(req.XpBase, ct);
        return resultado.ParaDto();
    }
}
