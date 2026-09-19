using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaRecompensaMissaoTests
{
    private readonly PoliticaRecompensaMissao _politica = new();

    [Theory]
    [InlineData(EsforcoMissao.Rapida, 10, 20)]
    [InlineData(EsforcoMissao.Media, 30, 50)]
    [InlineData(EsforcoMissao.Longa, 60, 100)]
    public void FaixaDe_DevolveOsLimitesCorretos(EsforcoMissao esforco, int min, int max)
    {
        var faixa = _politica.FaixaDe(esforco);

        Assert.Equal(min, faixa.Minimo);
        Assert.Equal(max, faixa.Maximo);
    }

    [Theory]
    [InlineData(EsforcoMissao.Rapida, 9, false)] // um a menos que o mínimo
    [InlineData(EsforcoMissao.Rapida, 10, true)] // limiar mínimo inclusivo
    [InlineData(EsforcoMissao.Rapida, 20, true)] // limiar máximo inclusivo
    [InlineData(EsforcoMissao.Rapida, 21, false)] // um a mais que o máximo
    [InlineData(EsforcoMissao.Media, 29, false)]
    [InlineData(EsforcoMissao.Media, 50, true)]
    public void XpValido_RespeitaOsLimitesFechadosDaFaixa(EsforcoMissao esforco, int xp, bool esperado)
    {
        Assert.Equal(esperado, _politica.XpValido(esforco, xp));
    }
}
