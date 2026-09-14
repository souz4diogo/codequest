using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>Sessões de foco Pomodoro (RF19). O timer roda no front; aqui só se registra o resultado.</summary>
[ApiController]
[Route("api/foco")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class FocoController : ControllerBase
{
    private readonly IFocoService _foco;

    public FocoController(IFocoService foco) => _foco = foco;

    /// <summary>Registra a sessão encerrada; credita XP (1/min planejado) só se completou (RF19).</summary>
    [HttpPost]
    public async Task<ActionResult<SessaoFocoDto>> Registrar(RegistrarFocoRequest req, CancellationToken ct)
    {
        var resultado = await _foco.RegistrarAsync(req.TopicoId, req.MinutosPlanejados, req.MinutosReais, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } sessao)
            return BadRequest(new { erro = resultado.Erro });

        return sessao.ParaDto();
    }
}
