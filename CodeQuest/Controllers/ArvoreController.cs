using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>Árvore de habilidades (RF06): módulos com status, tópicos e pré-requisitos.</summary>
[ApiController]
[Route("api/arvore")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ArvoreController : ControllerBase
{
    private readonly IArvoreService _arvore;
    private readonly IBossService _boss;

    public ArvoreController(IArvoreService arvore, IBossService boss)
    {
        _arvore = arvore;
        _boss = boss;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ModuloDto>>> Listar(CancellationToken ct)
    {
        var modulos = await _arvore.ListarAsync(ct);
        return Ok(modulos.Select(m => m.ParaDto()).ToList());
    }

    /// <summary>Inicia o boss fight do módulo (RN08): uma questão por tópico ativo, nível Expert.</summary>
    [HttpPost("modulos/{moduloId:int}/boss")]
    public async Task<ActionResult<TesteDto>> IniciarBoss(int moduloId, CancellationToken ct)
    {
        var resultado = await _boss.IniciarAsync(moduloId, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } teste)
            return BadRequest(new { erro = resultado.Erro });

        return teste.ParaDto();
    }

    /// <summary>Corrige o boss fight; aprovar (nota ≥ 70) conclui o módulo e libera os dependentes.</summary>
    [HttpPost("boss/{testeId:int}/responder")]
    public async Task<ActionResult<CorrecaoBossDto>> ResponderBoss(int testeId, ResponderTesteRequest req, CancellationToken ct)
    {
        var resultado = await _boss.ResponderAsync(testeId, req.Respostas, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } r)
            return BadRequest(new { erro = resultado.Erro });

        return r.ParaDto();
    }
}
