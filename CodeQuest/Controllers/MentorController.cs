using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>Chat de dúvidas com o mentor IA (RF22/RF23).</summary>
[ApiController]
[Route("api/duvidas")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class MentorController : ControllerBase
{
    private readonly IMentorService _mentor;

    public MentorController(IMentorService mentor) => _mentor = mentor;

    /// <summary>Histórico de dúvidas do aluno, mais recentes primeiro.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DuvidaDto>>> Listar(CancellationToken ct)
    {
        var duvidas = await _mentor.ListarAsync(ct);
        return Ok(duvidas.Select(d => d.ParaDto()).ToList());
    }

    /// <summary>Envia uma pergunta ao mentor; a resposta considera tópico e nível do aluno (RF22).</summary>
    [HttpPost]
    public async Task<ActionResult<DuvidaDto>> Perguntar(PerguntarRequest req, CancellationToken ct)
    {
        var resultado = await _mentor.PerguntarAsync(req.TopicoId, req.Pergunta, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } duvida)
            return BadRequest(new { erro = resultado.Erro });

        return duvida.ParaDto();
    }

    /// <summary>Marca/desmarca uma dúvida para revisão (RF23).</summary>
    [HttpPost("{id:int}/revisao")]
    public async Task<ActionResult<DuvidaDto>> MarcarRevisao(int id, MarcarRevisaoRequest req, CancellationToken ct)
    {
        var resultado = await _mentor.MarcarParaRevisaoAsync(id, req.Marcada, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } duvida)
            return BadRequest(new { erro = resultado.Erro });

        return duvida.ParaDto();
    }
}
