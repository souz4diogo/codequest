using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class CalculadoraRecompensaTests
{
    private readonly CalculadoraRecompensa _calc = new();

    [Theory]
    [InlineData(0, 1.0)]
    [InlineData(1, 1.1)]
    [InlineData(5, 1.5)] // teto de +50% (RN03)
    [InlineData(6, 1.5)] // acima do teto continua no teto
    [InlineData(100, 1.5)]
    public void MultiplicadorStreak_RespeitaOTetoDe50Porcento(int streak, double esperado)
    {
        Assert.Equal(esperado, _calc.MultiplicadorStreak(streak), precision: 5);
    }

    [Fact]
    public void AplicarMultiplicador_ArredondaParaOInteiroMaisProximo()
    {
        // 15 XP * 1.1 = 16.5 -> arredonda pra cima (AwayFromZero)
        Assert.Equal(17, _calc.AplicarMultiplicador(15, streakDias: 1));
    }

    [Fact]
    public void AplicarMultiplicador_SemStreak_NaoAlteraOXp()
    {
        Assert.Equal(100, _calc.AplicarMultiplicador(100, streakDias: 0));
    }

    [Fact]
    public void AplicarMultiplicador_XpNegativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calc.AplicarMultiplicador(-1, 0));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)] // arredonda pra cima (ceiling), gold nunca "some" por causa de XP baixo
    [InlineData(3, 1)]
    [InlineData(15, 5)]
    [InlineData(20, 7)]
    public void GoldPorXp_ArredondaSempreParaCima(int xp, int goldEsperado)
    {
        Assert.Equal(goldEsperado, _calc.GoldPorXp(xp));
    }

    [Fact]
    public void GoldPorXp_Negativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _calc.GoldPorXp(-5));
    }
}
