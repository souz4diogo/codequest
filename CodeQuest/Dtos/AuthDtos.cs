namespace CodeQuest.Dtos;

/// <summary>Contratos de entrada/saída da autenticação (o front nunca vê a entidade Usuario).</summary>
public sealed record RegistrarRequest(string Login, string Senha);

public sealed record LoginRequest(string Login, string Senha);

public sealed record RefreshRequest(string RefreshToken);

/// <summary>
/// Resposta de login/registro/refresh: o JWT que a SPA guarda e envia como Bearer, o refresh token
/// pra renovar sem pedir senha de novo, e metadados úteis.
/// </summary>
public sealed record TokenResponse(
    string Token,
    DateTime ExpiraEm,
    string RefreshToken,
    DateTime RefreshExpiraEm,
    string Login,
    int PlayerId);
