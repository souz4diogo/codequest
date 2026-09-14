using System.Net;
using CodeQuest.Dtos;
using CodeQuest.IntegrationTests.Suporte;
using CodeQuest.Models;
using Xunit;

namespace CodeQuest.IntegrationTests;

[Collection(CodeQuestCollection.Nome)]
public class LojaTests
{
    private readonly CodeQuestApiFactory _factory;

    public LojaTests(CodeQuestApiFactory factory) => _factory = factory;

    private async Task CreditarGoldAsync(int playerId, int gold)
    {
        await using var db = _factory.CriarDbContext();
        var player = await db.Players.FindAsync(playerId);
        player!.Gold = gold;
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task Itens_DevolveOCatalogoSemeado()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var itens = await cliente.ObterJsonAsync<List<ItemLojaDto>>("/api/loja/itens");

        Assert.NotNull(itens);
        Assert.True(itens!.Count >= 5);
        Assert.Contains(itens, i => i.Nome == "30 min de jogo" && i.CustoGold == 60);
    }

    [Fact]
    public async Task Comprar_SemGoldSuficiente_RetornaBadRequestENaoDebita()
    {
        var (cliente, token) = await _factory.RegistrarELogarAsync();
        var itens = await cliente.ObterJsonAsync<List<ItemLojaDto>>("/api/loja/itens");
        var item = itens!.First(i => i.Nome == "30 min de jogo");

        var resposta = await cliente.PostAsync($"/api/loja/comprar/{item.Id}", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var player = await cliente.ObterJsonAsync<PlayerDto>("/api/player");
        Assert.Equal(0, player!.Gold);
    }

    [Fact]
    public async Task Comprar_ComGoldSuficiente_DebitaOCusto()
    {
        var (cliente, token) = await _factory.RegistrarELogarAsync();
        await CreditarGoldAsync(token.PlayerId, 100);

        var itens = await cliente.ObterJsonAsync<List<ItemLojaDto>>("/api/loja/itens");
        var item = itens!.First(i => i.Nome == "30 min de jogo"); // custa 60

        var resposta = await cliente.PostAsync($"/api/loja/comprar/{item.Id}", null);
        resposta.EnsureSuccessStatusCode();
        var compra = await resposta.LerJsonAsync<CompraDto>();

        Assert.Equal(60, compra!.CustoGoldPago);
        Assert.Equal(40, compra.GoldRestante);
    }

    [Fact]
    public async Task Comprar_ItemInexistente_RetornaBadRequest()
    {
        var (cliente, _) = await _factory.RegistrarELogarAsync();

        var resposta = await cliente.PostAsync("/api/loja/comprar/999999", null);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }
}
