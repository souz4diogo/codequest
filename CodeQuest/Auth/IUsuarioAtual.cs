using System.Security.Claims;
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
/// Implementação sobre o <see cref="AuthenticationStateProvider"/> do Blazor — funciona tanto
/// no SSR estático (lê HttpContext.User) quanto no circuito interativo (estado do circuito).
/// </summary>
public sealed class UsuarioAtual : IUsuarioAtual
{
    private readonly AuthenticationStateProvider _authState;

    public UsuarioAtual(AuthenticationStateProvider authState) => _authState = authState;

    public async Task<int> ObterPlayerIdAsync(CancellationToken ct = default)
        => await ObterPlayerIdOuNuloAsync(ct)
           ?? throw new InvalidOperationException("Nenhum usuário autenticado no contexto atual.");

    public async Task<int?> ObterPlayerIdOuNuloAsync(CancellationToken ct = default)
    {
        var estado = await _authState.GetAuthenticationStateAsync();
        var claim = estado.User.FindFirst(ClaimsCodeQuest.PlayerId);
        return claim is not null && int.TryParse(claim.Value, out var id) ? id : null;
    }
}
