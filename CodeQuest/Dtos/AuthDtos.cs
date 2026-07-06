namespace CodeQuest.Dtos;

/// <summary>Contratos de entrada/saída da autenticação (o front nunca vê a entidade Usuario).</summary>
public sealed record RegistrarRequest(string Login, string Senha);

public sealed record LoginRequest(string Login, string Senha);

/// <summary>Resposta de login/registro: o JWT que a SPA guarda e envia como Bearer, mais metadados úteis.</summary>
public sealed record TokenResponse(string Token, DateTime ExpiraEm, string Login, int PlayerId);
