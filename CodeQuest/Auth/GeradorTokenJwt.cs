using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CodeQuest.Models;
using Microsoft.IdentityModel.Tokens;

namespace CodeQuest.Auth;

/// <summary>Configuração de emissão/validação dos tokens JWT (lida da config em <see cref="DependencyInjection"/>).</summary>
public sealed record OpcoesJwt(string Secret, string Issuer, string Audience, int ExpiraEmMinutos);

/// <summary>Token assinado + o instante em que expira (o front usa a expiração para saber quando renovar/deslogar).</summary>
public sealed record TokenGerado(string Token, DateTime ExpiraEm);

/// <summary>
/// Emite o JWT que a SPA React usa como Bearer: não há sessão no servidor — o token carrega
/// os claims e é validado a cada request.
/// </summary>
public interface IGeradorTokenJwt
{
    TokenGerado Gerar(Usuario usuario, int playerId);
}

public sealed class GeradorTokenJwt : IGeradorTokenJwt
{
    private readonly OpcoesJwt _opcoes;

    public GeradorTokenJwt(OpcoesJwt opcoes) => _opcoes = opcoes;

    public TokenGerado Gerar(Usuario usuario, int playerId)
    {
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opcoes.Secret));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(_opcoes.ExpiraEmMinutos);

        // Claims de ClaimsCodeQuest — inclui o PlayerId, que o IUsuarioAtual lê.
        var token = new JwtSecurityToken(
            issuer: _opcoes.Issuer,
            audience: _opcoes.Audience,
            claims: ClaimsCodeQuest.ConstruirClaims(usuario, playerId),
            expires: expiraEm,
            signingCredentials: credenciais);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new TokenGerado(jwt, expiraEm);
    }
}
