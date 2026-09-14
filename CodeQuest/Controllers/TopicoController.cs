using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>CRUD de tópicos de estudo (RF05): criar, editar, priorizar e arquivar.</summary>
[ApiController]
[Route("api/modulos/{moduloId:int}/topicos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class TopicoController : ControllerBase
{
    private readonly ITopicoService _topicos;

    public TopicoController(ITopicoService topicos) => _topicos = topicos;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TopicoGestaoDto>>> Listar(int moduloId, CancellationToken ct)
    {
        var topicos = await _topicos.ListarAsync(moduloId, ct);
        return Ok(topicos.Select(t => t.ParaGestaoDto()).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<TopicoGestaoDto>> Criar(int moduloId, SalvarTopicoRequest req, CancellationToken ct)
    {
        var resultado = await _topicos.CriarAsync(moduloId, new DadosTopico(req.Nome, req.Prioridade), ct);
        if (!resultado.Sucesso || resultado.Valor is not { } topico)
            return BadRequest(new { erro = resultado.Erro });

        return topico.ParaGestaoDto();
    }

    [HttpPut("{topicoId:int}")]
    public async Task<ActionResult<TopicoGestaoDto>> Editar(int moduloId, int topicoId, SalvarTopicoRequest req, CancellationToken ct)
    {
        var resultado = await _topicos.EditarAsync(moduloId, topicoId, new DadosTopico(req.Nome, req.Prioridade), ct);
        if (!resultado.Sucesso || resultado.Valor is not { } topico)
            return BadRequest(new { erro = resultado.Erro });

        return topico.ParaGestaoDto();
    }

    [HttpPost("{topicoId:int}/arquivar")]
    public async Task<ActionResult<TopicoGestaoDto>> Arquivar(int moduloId, int topicoId, ArquivarTopicoRequest req, CancellationToken ct)
    {
        var resultado = await _topicos.ArquivarAsync(moduloId, topicoId, req.Arquivado, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } topico)
            return BadRequest(new { erro = resultado.Erro });

        return topico.ParaGestaoDto();
    }
}
