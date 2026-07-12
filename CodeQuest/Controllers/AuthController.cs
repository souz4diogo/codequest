using CodeQuest.Auth;
using CodeQuest.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CodeQuest.Controllers;

/// <summary>
/// Autenticação da API React: valida as credenciais e devolve um JWT que a SPA guarda e reenvia
/// no header <c>Authorization</c>. A regra de domínio continua em <see cref="IServicoAutenticacao"/>
/// — o controller só orquestra.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IServicoAutenticacao _auth;
    private readonly IGeradorTokenJwt _token;

    public AuthController(IServicoAutenticacao auth, IGeradorTokenJwt token)
    {
        _auth = auth;
        _token = token;
    }

    [HttpPost("registrar")]
    public async Task<ActionResult<TokenResponse>> Registrar(RegistrarRequest req, CancellationToken ct)
    {
        var resultado = await _auth.RegistrarAsync(req.Login, req.Senha, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } usuario)
            return BadRequest(new { erro = resultado.Erro });

        return Emitir(usuario);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest req, CancellationToken ct)
    {
        var resultado = await _auth.ValidarCredenciaisAsync(req.Login, req.Senha, ct);
        if (!resultado.Sucesso || resultado.Valor is not { } usuario)
            return Unauthorized(new { erro = resultado.Erro });

        return Emitir(usuario);
    }

    private TokenResponse Emitir(Models.Usuario usuario)
    {
        var playerId = usuario.Player!.Id;
        var token = _token.Gerar(usuario, playerId);
        return new TokenResponse(token.Token, token.ExpiraEm, usuario.Login, playerId);
    }
}
