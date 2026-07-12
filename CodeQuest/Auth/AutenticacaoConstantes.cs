using System.Security.Claims;
using CodeQuest.Models;

namespace CodeQuest.Auth;

/// <summary>
/// Nomes de claims e a fábrica do <see cref="ClaimsPrincipal"/> do usuário autenticado.
/// Centralizado aqui para que o gerador do JWT (embute as claims) e <see cref="IUsuarioAtual"/>
/// (lê as claims) concordem sobre os nomes — fonte única de verdade.
/// </summary>
public static class ClaimsCodeQuest
{
    /// <summary>Claim com o Id do Player associado ao usuário — evita ir ao banco a cada request.</summary>
    public const string PlayerId = "codequest:player_id";

    /// <summary>
    /// Claims que identificam o usuário, embutidas no JWT que a SPA React envia a cada request.
    /// </summary>
    public static IReadOnlyList<Claim> ConstruirClaims(Usuario usuario, int playerId) =>
        new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Login),
            new(PlayerId, playerId.ToString()),
        };
}
