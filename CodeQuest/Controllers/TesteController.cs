using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>Testes de avaliação por tópico (RF17): 5–10 questões geradas pela IA.</summary>
[ApiController]
[Route("api/testes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class TesteController : ControllerBase
{
    private readonly ITesteService _testes;

    public TesteController(ITesteService testes) => _testes = testes;

    [HttpPost]
    public async Task<ActionResult<TesteDto>> Iniciar(IniciarTesteRequest req, CancellationToken ct)
    {
        var resultado = await _testes.IniciarAsync(req.TopicoId, req.Dificuldade, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } teste)
            return BadRequest(new { erro = resultado.Erro });

        return teste.ParaDto();
    }

    [HttpPost("{id:int}/responder")]
    public async Task<ActionResult<CorrecaoTesteDto>> Responder(int id, ResponderTesteRequest req, CancellationToken ct)
    {
        var resultado = await _testes.ResponderAsync(id, req.Respostas, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } r)
            return BadRequest(new { erro = resultado.Erro });

        return r.ParaDto();
    }
}
