using CodeQuest.Dtos;
using CodeQuest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>Dados agregados do painel (RF08/RF24): atividade recente e radar de habilidades.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class DashboardController : ControllerBase
{
    private const int DiasAtividadePadrao = 30;

    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    /// <summary>Quantidade de atividades concluídas por dia, últimos 30 dias (RF24).</summary>
    [HttpGet("atividade")]
    public async Task<ActionResult<IReadOnlyList<AtividadeDoDiaDto>>> Atividade(CancellationToken ct)
    {
        var atividade = await _dashboard.ObterAtividadeAsync(DiasAtividadePadrao, ct);
        return Ok(atividade.Select(a => a.ParaDto()).ToList());
    }

    /// <summary>Nível médio por módulo, para o radar de habilidades (RF08).</summary>
    [HttpGet("radar")]
    public async Task<ActionResult<IReadOnlyList<NivelPorModuloDto>>> Radar(CancellationToken ct)
    {
        var radar = await _dashboard.ObterRadarAsync(ct);
        return Ok(radar.Select(r => r.ParaDto()).ToList());
    }
}
