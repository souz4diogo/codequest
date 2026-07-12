namespace CodeQuest.Auth;

/// <summary>
/// Resolve quem é o usuário/player logado a partir dos claims do JWT. Os serviços de
/// aplicação dependem desta abstração (DIP) em vez de assumir um player fixo — é o que
/// torna o app multi-usuário sem espalhar leitura de claims pelo domínio.
/// </summary>
public interface IUsuarioAtual
{
    /// <summary>Id do Player logado. Lança se não houver usuário autenticado.</summary>
    Task<int> ObterPlayerIdAsync(CancellationToken ct = default);

    /// <summary>Id do Player logado, ou null se anônimo.</summary>
    Task<int?> ObterPlayerIdOuNuloAsync(CancellationToken ct = default);
}

/// <summary>
/// Lê o player logado a partir do <c>HttpContext.User</c>, preenchido pela validação do JWT
/// Bearer que a SPA React envia no header <c>Authorization</c> a cada requisição.
/// </summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _httpContext;

    public UsuarioAtual(IHttpContextAccessor httpContext) => _httpContext = httpContext;

    public async Task<int> ObterPlayerIdAsync(CancellationToken ct = default)
        => await ObterPlayerIdOuNuloAsync(ct)
           ?? throw new InvalidOperationException("Nenhum usuário autenticado no contexto atual.");

    public Task<int?> ObterPlayerIdOuNuloAsync(CancellationToken ct = default)
    {
        var usuario = _httpContext.HttpContext?.User;
        var claim = usuario?.FindFirst(ClaimsCodeQuest.PlayerId);
        var id = claim is not null && int.TryParse(claim.Value, out var valor) ? valor : (int?)null;
        return Task.FromResult(id);
    }
}
