using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>
/// Exercícios gerados e corrigidos pela IA (RF14/RF15/RF18 — Fase 2). O controller só
/// orquestra: prompts vivem em <c>Prompts</c>, regras de jogo nos services (arquitetura da
/// seção 3 do doc de requisitos). O front nunca fala com o Gemini — só com esta API (RNF02).
/// </summary>
[ApiController]
[Route("api/exercicios")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ExercicioController : ControllerBase
{
    private readonly IExercicioService _exercicios;

    public ExercicioController(IExercicioService exercicios) => _exercicios = exercicios;

    /// <summary>Gera um exercício por tópico + dificuldade + formato (RF14) e o persiste (RF18).</summary>
    [HttpPost("gerar")]
    public async Task<ActionResult<ExercicioDto>> Gerar(GerarExercicioRequest req, CancellationToken ct)
    {
        var resultado = await _exercicios.GerarAsync(req.TopicoId, req.Dificuldade, req.Formato, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } exercicio)
            return BadRequest(new { erro = resultado.Erro });

        return exercicio.ParaDto();
    }

    /// <summary>
    /// Corrige a resposta (RF15): nota e feedback da IA; XP (RN09) e revisão (RN06) decididos
    /// pelo backend. Devolve o parecer completo para o front exibir.
    /// </summary>
    [HttpPost("{id:int}/responder")]
    public async Task<ActionResult<CorrecaoDto>> Responder(int id, ResponderExercicioRequest req, CancellationToken ct)
    {
        var resultado = await _exercicios.ResponderAsync(id, req.Resposta, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } tentativa)
            return BadRequest(new { erro = resultado.Erro });

        return tentativa.ParaDto();
    }
}
