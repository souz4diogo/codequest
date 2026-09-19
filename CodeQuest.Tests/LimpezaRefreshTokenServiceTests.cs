using CodeQuest.Auth;
using CodeQuest.Models;
using CodeQuest.Tests.Suporte;
using Xunit;

namespace CodeQuest.Tests;

public class LimpezaRefreshTokenServiceTests
{
    private static async Task<int> CriarUsuarioAsync(CodeQuest.Data.AppDbContext db)
    {
        var usuario = new Usuario { Login = $"user_{Guid.NewGuid():N}", SenhaHash = "hash" };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario.Id;
    }

    [Fact]
    public async Task LimparAsync_ApagaTokenExpirado_MantemOValido()
    {
        using var db = DbContextFactory.Criar();
        var relogio = new RelogioFake();
        var usuarioId = await CriarUsuarioAsync(db);

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = "expirado",
            CriadoEm = relogio.Agora.AddDays(-31),
            ExpiraEm = relogio.Agora.AddDays(-1), // já passou
        });
        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = "valido",
            CriadoEm = relogio.Agora,
            ExpiraEm = relogio.Agora.AddDays(29), // ainda longe de expirar
        });
        await db.SaveChangesAsync();

        var apagados = await LimpezaRefreshTokenService.LimparAsync(db, relogio);

        Assert.Equal(1, apagados);
        var restante = Assert.Single(db.RefreshTokens);
        Assert.Equal("valido", restante.TokenHash);
    }

    [Fact]
    public async Task LimparAsync_ApagaTokenRevogadoMesmoQueAindaNaoTenhaExpirado()
    {
        using var db = DbContextFactory.Criar();
        var relogio = new RelogioFake();
        var usuarioId = await CriarUsuarioAsync(db);

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = "revogado-mas-nao-expirado",
            CriadoEm = relogio.Agora,
            ExpiraEm = relogio.Agora.AddDays(29),
            RevogadoEm = relogio.Agora,
        });
        await db.SaveChangesAsync();

        var apagados = await LimpezaRefreshTokenService.LimparAsync(db, relogio);

        Assert.Equal(1, apagados);
        Assert.Empty(db.RefreshTokens);
    }

    [Fact]
    public async Task LimparAsync_SemNadaPraLimpar_NaoApagaNada()
    {
        using var db = DbContextFactory.Criar();
        var relogio = new RelogioFake();
        var usuarioId = await CriarUsuarioAsync(db);

        db.RefreshTokens.Add(new RefreshToken
        {
            UsuarioId = usuarioId,
            TokenHash = "valido",
            CriadoEm = relogio.Agora,
            ExpiraEm = relogio.Agora.AddDays(29),
        });
        await db.SaveChangesAsync();

        var apagados = await LimpezaRefreshTokenService.LimparAsync(db, relogio);

        Assert.Equal(0, apagados);
        Assert.Single(db.RefreshTokens);
    }
}
