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

    /// <summary>
    /// Claims que identificam o usuário — compartilhadas pelo cookie (Blazor) e pelo JWT (API React),
    /// para que os dois formatos de autenticação carreguem exatamente a mesma informação.
    /// </summary>
    public static IReadOnlyList<Claim> ConstruirClaims(Usuario usuario, int playerId) =>
        new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Login),
            new(PlayerId, playerId.ToString()),
        };

    /// <summary>Monta a identidade do usuário para o cookie de autenticação.</summary>
    public static ClaimsPrincipal ConstruirPrincipal(Usuario usuario, int playerId)
    {
        var identidade = new ClaimsIdentity(ConstruirClaims(usuario, playerId), authenticationType: "CodeQuestCookie");
        return new ClaimsPrincipal(identidade);
    }
}
