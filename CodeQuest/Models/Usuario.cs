namespace CodeQuest.Models;

/// <summary>
/// Credencial de acesso (RF de autenticação). Guarda apenas o login e o hash da senha —
/// a senha em texto puro nunca é persistida. Cada usuário tem exatamente um <see cref="Player"/>
/// (relação 1:1), criado no momento do registro.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    /// <summary>Identificador de login (único, case-insensitive na prática — normalizado no serviço).</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Hash da senha (PBKDF2 via PasswordHasher). Nunca guarda a senha em claro.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navegação 1:1 — o Player é o lado dependente (carrega a FK UsuarioId).
    public Player? Player { get; set; }
}
