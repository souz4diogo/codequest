using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class ReguladorNivelTests
{
    private readonly ReguladorNivel _regulador = new();

    [Theory]
    [InlineData(0, 1)]
    [InlineData(49, 1)]
    [InlineData(199, 1)] // ainda não bateu os 200 do nível 2
    [InlineData(200, 2)] // âncora do plano: nível 2 = 200 XP
    [InlineData(1250, 5)] // âncora do plano: nível 5 = 1250 XP
    [InlineData(5000, 10)] // âncora do plano: nível 10 = 5000 XP
    public void CalcularNivel_AncorasDoPlano(int xp, int nivelEsperado)
    {
        Assert.Equal(nivelEsperado, _regulador.CalcularNivel(xp));
    }

    [Fact]
    public void CalcularNivel_XpNegativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _regulador.CalcularNivel(-1));
    }

    [Fact]
    public void XpParaNivel_Nivel1_EhZero()
    {
        Assert.Equal(0, _regulador.XpParaNivel(1));
    }

    [Fact]
    public void XpParaNivel_Nivel2_BateComAAncora()
    {
        Assert.Equal(200, _regulador.XpParaNivel(2));
    }

    [Fact]
    public void XpParaNivel_NivelMenorQue1_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _regulador.XpParaNivel(0));
    }

    [Fact]
    public void XpFaltandoParaProximoNivel_UmXpAntesDeSubir_Falta1()
    {
        // 199 XP ainda é nível 1; falta só 1 XP pra bater os 200 do nível 2.
        Assert.Equal(1, _regulador.XpFaltandoParaProximoNivel(199));
    }

    [Fact]
    public void XpFaltandoParaProximoNivel_LogoAposSubir_JaContaProOProximoNivelSeguinte()
    {
        // Ao bater 200 (nível 2), a meta vira o nível 3 (450 XP) — nunca falta 0, porque
        // bater a meta exata já é o que faz o nível subir.
        Assert.Equal(250, _regulador.XpFaltandoParaProximoNivel(200));
    }
}
