using System.Net;
using System.Net.Http.Json;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class AuthEPlayerTests
{
    private readonly CodeQuestApiFactory _factory;

    public AuthEPlayerTests(CodeQuestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Registrar_UsuarioNovo_CriaPlayerComNivel1EDevolveToken()
    {
        var (cliente, token) = await _factory.RegistrarELogarAsync();

        Assert.False(string.IsNullOrWhiteSpace(token.Token));
        Assert.True(token.PlayerId > 0);

        var player = await cliente.GetFromJsonAsync<PlayerDto>("/api/player");

        Assert.NotNull(player);
        Assert.Equal(1, player!.Nivel);
        Assert.Equal(0, player.XpTotal);
        Assert.Equal(0, player.Gold);
    }

    [Fact]
    public async Task Registrar_LoginJaEmUso_RetornaBadRequest()
    {
        var login = $"user_{Guid.NewGuid():N}";
        await _factory.RegistrarELogarAsync(login);

        var cliente = _factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/auth/registrar", new RegistrarRequest(login, "outrasenha"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_SenhaErrada_RetornaUnauthorized()
    {
        var login = $"user_{Guid.NewGuid():N}";
        await _factory.RegistrarELogarAsync(login, "senhaCorreta1");

        var cliente = _factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(login, "senhaErrada1"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisCorretas_DevolveMesmoPlayerIdDoRegistro()
    {
        var login = $"user_{Guid.NewGuid():N}";
        var (_, tokenRegistro) = await _factory.RegistrarELogarAsync(login, "senhaCorreta1");

        var cliente = _factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/auth/login", new LoginRequest(login, "senhaCorreta1"));
        resposta.EnsureSuccessStatusCode();
        var tokenLogin = await resposta.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(tokenRegistro.PlayerId, tokenLogin!.PlayerId);
    }

    [Fact]
    public async Task Registrar_SenhaCurta_RetornaBadRequest()
    {
        var cliente = _factory.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/api/auth/registrar",
            new RegistrarRequest($"user_{Guid.NewGuid():N}", "123"));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_LoginComCaixaEEspacosDiferentes_FuncionaMesmoAssim()
    {
        var login = $"User_{Guid.NewGuid():N}";
        await _factory.RegistrarELogarAsync(login, "senhaCorreta1");

        var cliente = _factory.CreateClient();
        var resposta = await cliente.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"  {login.ToUpperInvariant()}  ", "senhaCorreta1"));

        Assert.True(resposta.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Login_UsuarioInexistente_RetornaUnauthorized()
    {
        var cliente = _factory.CreateClient();

        var resposta = await cliente.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"nao_existe_{Guid.NewGuid():N}", "qualquersenha"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Player_SemToken_RetornaUnauthorized()
    {
        var cliente = _factory.CreateClient();

        var resposta = await cliente.GetAsync("/api/player");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task AdicionarXp_CreditaXpERetornaEstadoAtualizado()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/player/xp", new AdicionarXpRequest(50));
        resposta.EnsureSuccessStatusCode();
        var resultado = await resposta.Content.ReadFromJsonAsync<ResultadoXpDto>();

        Assert.NotNull(resultado);
        Assert.True(resultado!.XpCreditado > 0);

        var player = await cliente.GetFromJsonAsync<PlayerDto>("/api/player");
        Assert.Equal(resultado.XpCreditado, player!.XpTotal);
    }

    [Fact]
    public async Task AdicionarXp_ValorNegativo_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostAsJsonAsync("/api/player/xp", new AdicionarXpRequest(-10));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task DoisUsuarios_TemPlayersIndependentes()
    {
        var (clienteA, tokenA) = await _factory.RegistrarELogarAsync();
        var (clienteB, tokenB) = await _factory.RegistrarELogarAsync();

        Assert.NotEqual(tokenA.PlayerId, tokenB.PlayerId);

        await clienteA.PostAsJsonAsync("/api/player/xp", new AdicionarXpRequest(100));

        var playerA = await clienteA.GetFromJsonAsync<PlayerDto>("/api/player");
        var playerB = await clienteB.GetFromJsonAsync<PlayerDto>("/api/player");

        Assert.True(playerA!.XpTotal > 0);
        Assert.Equal(0, playerB!.XpTotal);
    }
}
