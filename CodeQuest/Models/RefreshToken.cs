namespace CodeQuest.Models;

/// <summary>
/// Refresh token opaco (RF de autenticação): permite renovar o JWT sem pedir login de novo,
/// mantendo a sessão sem token de vida longa circulando. Guarda só o hash — o texto original
/// nunca é persistido, só devolvido uma vez na resposta de login/registro/refresh.
/// </summary>
public class RefreshToken
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    /// <summary>SHA-256 (hex) do token — permite localizar por igualdade sem guardar o segredo em claro.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }

    /// <summary>Preenchido quando revogado (logout ou rotação no refresh) — token de uso único.</summary>
    public DateTime? RevogadoEm { get; set; }
}
