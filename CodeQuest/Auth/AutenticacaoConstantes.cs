using System.Security.Claims;
using CodeQuest.Models;

namespace CodeQuest.Auth;

/// <summary>
/// Nomes de claims e a fábrica do <see cref="ClaimsPrincipal"/> do usuário autenticado.
/// Centralizado aqui para que login (cria o principal) e <see cref="IUsuarioAtual"/> (lê o
/// principal) concordem sobre os nomes — fonte única de verdade.
/// </summary>
public static class ClaimsCodeQuest
{
    /// <summary>Claim com o Id do Player associado ao usuário — evita ir ao banco a cada request.</summary>
    public const string PlayerId = "codequest:player_id";

    /// <summary>Monta a identidade do usuário para o cookie de autenticação.</summary>
    public static ClaimsPrincipal ConstruirPrincipal(Usuario usuario, int playerId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Login),
            new(PlayerId, playerId.ToString()),
        };
        var identidade = new ClaimsIdentity(claims, authenticationType: "CodeQuestCookie");
        return new ClaimsPrincipal(identidade);
    }
}
