using CodeQuest.Dtos;
using CodeQuest.Services;
using CodeQuest.Services.Regras;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>
/// Missões do jogador logado (RF09–RF13). O <see cref="IMissaoService"/> resolve o player via
/// <c>IUsuarioAtual</c> a partir do JWT — o controller nunca recebe id de player do cliente.
/// </summary>
[ApiController]
[Route("api/missoes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class MissaoController : ControllerBase
{
    private readonly IMissaoService _missoes;
    private readonly IPoliticaRecompensaMissao _politica;

    public MissaoController(IMissaoService missoes, IPoliticaRecompensaMissao politica)
    {
        _missoes = missoes;
        _politica = politica;
    }

    /// <summary>Missões do dia atual do jogador.</summary>
    [HttpGet("dia")]
    public async Task<ActionResult<IReadOnlyList<MissaoDto>>> ListarDoDia(CancellationToken ct)
    {
        var missoes = await _missoes.ListarDoDiaAsync(ct);
        return Ok(missoes.Select(m => m.ParaDto()).ToList());
    }

    /// <summary>Faixas de XP por esforço (RN04) — o front usa para montar e validar o formulário.</summary>
    [HttpGet("faixas")]
    public ActionResult<IReadOnlyList<FaixaEsforcoDto>> Faixas()
    {
        var faixas = Enum.GetValues<EsforcoMissao>()
            .Select(e =>
            {
                var f = _politica.FaixaDe(e);
                return new FaixaEsforcoDto(e, f.Minimo, f.Maximo);
            })
            .ToList();
        return Ok(faixas);
    }

    /// <summary>Cria uma missão manual (RF09). O XP é validado contra a faixa do esforço (RN04).</summary>
    [HttpPost]
    public async Task<ActionResult<MissaoDto>> Criar(CriarMissaoRequest req, CancellationToken ct)
    {
        var dados = new NovaMissaoManual(req.Titulo, req.Esforco, req.Xp, req.Descricao, req.TopicoId);
        var resultado = await _missoes.CriarManualAsync(dados, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } missao)
            return BadRequest(new { erro = resultado.Erro });

        return CreatedAtAction(nameof(ListarDoDia), missao.ParaDto());
    }

    /// <summary>Estrutura uma missão a partir de texto livre (RF10): a IA sugere título/descrição/esforço,
    /// o XP sai sempre da tabela do backend (RN09).</summary>
    [HttpPost("sugerir")]
    public async Task<ActionResult<MissaoDto>> Sugerir(SugerirMissaoRequest req, CancellationToken ct)
    {
        var resultado = await _missoes.SugerirAsync(req.Texto, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } missao)
            return BadRequest(new { erro = resultado.Erro });

        return CreatedAtAction(nameof(ListarDoDia), missao.ParaDto());
    }

    /// <summary>Conclui uma missão e credita XP+gold. Devolve o detalhe da recompensa (subiu de nível, streak…).</summary>
    [HttpPost("{id:int}/concluir")]
    public async Task<ActionResult<ResultadoXpDto>> Concluir(int id, CancellationToken ct)
    {
        var resultado = await _missoes.ConcluirAsync(id, ct);
        if (!resultado.Sucesso)
            return BadRequest(new { erro = resultado.Erro });

        return resultado.Valor.ParaDto();
    }
}
