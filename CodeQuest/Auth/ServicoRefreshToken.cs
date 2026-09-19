using System.Security.Cryptography;
using CodeQuest.Common;
using CodeQuest.Data;
using CodeQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeQuest.Auth;

/// <summary>Refresh token recém-emitido: o texto (devolvido uma única vez) e quando expira.</summary>
public sealed record RefreshTokenGerado(string Token, DateTime ExpiraEm);

/// <summary>
/// Emissão/rotação/revogação de refresh tokens (RF de autenticação): guarda só o hash no banco
/// e devolve o texto original apenas na hora da emissão — se vazar o banco, o token não é
/// reutilizável. Rotação em uso único: cada <see cref="RotacionarAsync"/> revoga o token
/// apresentado e emite um novo, então um token roubado e já usado pelo dono vira sinal de replay.
/// </summary>
public interface IServicoRefreshToken
{
    Task<RefreshTokenGerado> EmitirAsync(int usuarioId, CancellationToken ct = default);

    /// <summary>Troca um refresh token válido por um novo, revogando o apresentado (rotação).</summary>
    Task<Resultado<(Usuario Usuario, RefreshTokenGerado Token)>> RotacionarAsync(string tokenTexto, CancellationToken ct = default);

    Task RevogarAsync(string tokenTexto, CancellationToken ct = default);
}

public sealed class ServicoRefreshToken : IServicoRefreshToken
{
    private readonly AppDbContext _db;
    private readonly IRelogio _relogio;
    private readonly OpcoesJwt _opcoes;

    public ServicoRefreshToken(AppDbContext db, IRelogio relogio, OpcoesJwt opcoes)
    {
        _db = db;
        _relogio = relogio;
        _opcoes = opcoes;
    }

    public async Task<RefreshTokenGerado> EmitirAsync(int usuarioId, CancellationToken ct = default)
    {
        var (texto, hash) = GerarParDeToken();
        var agora = _relogio.Agora;
        var expiraEm = agora.AddDays(_opcoes.RefreshExpiraEmDias);

        _db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = hash,
            CriadoEm = agora,
            ExpiraEm = expiraEm,
        });
        await _db.SaveChangesAsync(ct);

        return new RefreshTokenGerado(texto, expiraEm);
    }

    public async Task<Resultado<(Usuario Usuario, RefreshTokenGerado Token)>> RotacionarAsync(
        string tokenTexto, CancellationToken ct = default)
    {
        var registro = await LocalizarValidoAsync(tokenTexto, ct);
        if (registro is null)
            return Resultado.Falha<(Usuario, RefreshTokenGerado)>("Refresh token inválido ou expirado.");

        registro.RevogadoEm = _relogio.Agora;
        var novo = await EmitirAsync(registro.UsuarioId, ct);

        return Resultado.Ok((registro.Usuario, novo));
    }

    public async Task RevogarAsync(string tokenTexto, CancellationToken ct = default)
    {
        var registro = await LocalizarValidoAsync(tokenTexto, ct);
        if (registro is null) return; // Logout é idempotente: token já inválido não é erro.

        registro.RevogadoEm = _relogio.Agora;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<RefreshToken?> LocalizarValidoAsync(string tokenTexto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tokenTexto)) return null;

        var hash = Hash(tokenTexto);
        var registro = await _db.RefreshTokens
            .Include(r => r.Usuario).ThenInclude(u => u.Player)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);

        if (registro is null || registro.RevogadoEm is not null || registro.ExpiraEm <= _relogio.Agora)
            return null;

        return registro;
    }

    private static (string Texto, string Hash) GerarParDeToken()
    {
        var texto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (texto, Hash(texto));
    }

    private static string Hash(string texto) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(texto)));
}
