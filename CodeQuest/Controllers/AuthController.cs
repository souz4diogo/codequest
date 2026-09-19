using CodeQuest.Auth;
using CodeQuest.Dtos;
using CodeQuest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CodeQuest.Controllers;

/// <summary>
/// Autenticação da API React: valida as credenciais e devolve um JWT + refresh token que a SPA
/// guarda e reenvia. A regra de domínio continua em <see cref="IServicoAutenticacao"/>/
/// <see cref="IServicoRefreshToken"/> — o controller só orquestra.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IServicoAutenticacao _auth;
    private readonly IGeradorTokenJwt _token;
    private readonly IServicoRefreshToken _refresh;

    public AuthController(IServicoAutenticacao auth, IGeradorTokenJwt token, IServicoRefreshToken refresh)
    {
        _auth = auth;
        _token = token;
        _refresh = refresh;
    }

    [HttpPost("registrar")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenResponse>> Registrar(RegistrarRequest req, CancellationToken ct)
    {
        var resultado = await _auth.RegistrarAsync(req.Login, req.Senha, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } usuario)
            return BadRequest(new { erro = resultado.Erro });

        return await EmitirAsync(usuario, ct);
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var resultado = await _auth.ValidarCredenciaisAsync(req.Login, req.Senha, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } usuario)
            return Unauthorized(new { erro = resultado.Erro });

        return await EmitirAsync(usuario, ct);
    }

    /// <summary>Troca um refresh token válido por um novo par (rotação) sem pedir senha de novo.</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(RefreshRequest req, CancellationToken ct)
    {
        var resultado = await _refresh.RotacionarAsync(req.RefreshToken, ct);
        if (!resultado.Sucesso)
            return Unauthorized(new { erro = resultado.Erro });

        var (usuario, refreshToken) = resultado.Valor;
        return Responder(usuario, refreshToken);
    }

    /// <summary>Revoga o refresh token no servidor — o Bearer atual continua válido até expirar.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshRequest req, CancellationToken ct)
    {
        await _refresh.RevogarAsync(req.RefreshToken, ct);
        return NoContent();
    }

    private async Task<TokenResponse> EmitirAsync(Usuario usuario, CancellationToken ct)
    {
        var refreshToken = await _refresh.EmitirAsync(usuario.Id, ct);
        return Responder(usuario, refreshToken);
    }

    private TokenResponse Responder(Usuario usuario, RefreshTokenGerado refreshToken)
    {
        var playerId = usuario.Player!.Id;
        var token = _token.Gerar(usuario, playerId);
        return new TokenResponse(
            token.Token, token.ExpiraEm,
            refreshToken.Token, refreshToken.ExpiraEm,
            usuario.Login, playerId);
    }
}
