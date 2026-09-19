using CodeQuest.Auth;
using CodeQuest.Models;
using CodeQuest.Tests.Suporte;
using Xunit;

namespace CodeQuest.Tests;

public class ServicoRefreshTokenTests
{
    private static readonly OpcoesJwt Opcoes = new("segredo-de-teste-com-32-caracteres!", "CodeQuest", "CodeQuest", 60, RefreshExpiraEmDias: 30);

    private static async Task<int> CriarUsuarioAsync(CodeQuest.Data.AppDbContext db)
    {
        var usuario = new Usuario { Login = "player1", SenhaHash = "hash" };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return usuario.Id;
    }

    [Fact]
    public async Task EmitirAsync_GeraTokenComExpiracaoNoFuturo()
    {
        using var db = DbContextFactory.Criar();
        var usuarioId = await CriarUsuarioAsync(db);
        var relogio = new RelogioFake();
        var service = new ServicoRefreshToken(db, relogio, Opcoes);

        var token = await service.EmitirAsync(usuarioId);

        Assert.False(string.IsNullOrWhiteSpace(token.Token));
        Assert.Equal(relogio.Agora.AddDays(30), token.ExpiraEm);
    }

    [Fact]
    public async Task RotacionarAsync_TokenValido_RevogaOAntigoEEmiteNovo()
    {
        using var db = DbContextFactory.Criar();
        var usuarioId = await CriarUsuarioAsync(db);
        var service = new ServicoRefreshToken(db, new RelogioFake(), Opcoes);
        var original = await service.EmitirAsync(usuarioId);

        var resultado = await service.RotacionarAsync(original.Token);
        Assert.True(resultado.Sucesso);
        Assert.NotEqual(original.Token, resultado.Valor.Token.Token);

        // Uso único: reapresentar o token já rotacionado falha (indício de replay).
        var reuso = await service.RotacionarAsync(original.Token);
        Assert.False(reuso.Sucesso);
    }

    [Fact]
    public async Task RotacionarAsync_TokenExpirado_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var usuarioId = await CriarUsuarioAsync(db);
        var relogio = new RelogioFake();
        var service = new ServicoRefreshToken(db, relogio, Opcoes);
        var token = await service.EmitirAsync(usuarioId);

        relogio.Agora = relogio.Agora.AddDays(31); // passou dos 30 dias de validade

        var resultado = await service.RotacionarAsync(token.Token);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task RotacionarAsync_TokenDesconhecido_RetornaFalha()
    {
        using var db = DbContextFactory.Criar();
        var service = new ServicoRefreshToken(db, new RelogioFake(), Opcoes);

        var resultado = await service.RotacionarAsync("token-que-nunca-existiu");

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task RevogarAsync_TokenValido_ImpedeRotacaoFutura()
    {
        using var db = DbContextFactory.Criar();
        var usuarioId = await CriarUsuarioAsync(db);
        var service = new ServicoRefreshToken(db, new RelogioFake(), Opcoes);
        var token = await service.EmitirAsync(usuarioId);

        await service.RevogarAsync(token.Token);
        var resultado = await service.RotacionarAsync(token.Token);

        Assert.False(resultado.Sucesso);
    }

    [Fact]
    public async Task RevogarAsync_TokenDesconhecido_NaoLancaExcecao()
    {
        using var db = DbContextFactory.Criar();
        var service = new ServicoRefreshToken(db, new RelogioFake(), Opcoes);

        await service.RevogarAsync("token-que-nunca-existiu"); // idempotente, sem exceção
    }
}
