using Microsoft.AspNetCore.Components.Authorization;

namespace CodeQuest.Auth;

/// <summary>
/// Resolve quem é o usuário/player logado a partir dos claims do cookie. Os serviços de
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
/// Resolve o usuário logado nos dois mundos que coexistem durante a migração para React:
/// <list type="bullet">
///   <item>Requisição HTTP (API com JWT, ou render estático do Blazor): lê <c>HttpContext.User</c>.</item>
///   <item>Circuito interativo do Blazor Server (sem HttpContext): cai no <see cref="AuthenticationStateProvider"/>.</item>
/// </list>
/// Quando o Blazor for totalmente removido, o fallback e a dependência de <see cref="AuthenticationStateProvider"/>
/// saem, restando apenas a leitura do <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _httpContext;
    private readonly AuthenticationStateProvider _authState;

    public UsuarioAtual(IHttpContextAccessor httpContext, AuthenticationStateProvider authState)
    {
        _httpContext = httpContext;
        _authState = authState;
    }

    public async Task<int> ObterPlayerIdAsync(CancellationToken ct = default)
        => await ObterPlayerIdOuNuloAsync(ct)
           ?? throw new InvalidOperationException("Nenhum usuário autenticado no contexto atual.");

    public async Task<int?> ObterPlayerIdOuNuloAsync(CancellationToken ct = default)
    {
        var usuario = _httpContext.HttpContext?.User;

        // Sem HttpContext autenticado → estamos num circuito interativo do Blazor.
        if (usuario?.Identity?.IsAuthenticated != true)
            usuario = (await _authState.GetAuthenticationStateAsync()).User;

        var claim = usuario.FindFirst(ClaimsCodeQuest.PlayerId);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
    }
}
