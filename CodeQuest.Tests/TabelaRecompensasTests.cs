using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class TabelaRecompensasTests
{
    private readonly TabelaRecompensas _tabela = new();

    [Theory]
    [InlineData(Dificuldade.Easy, 15)]
    [InlineData(Dificuldade.Medium, 30)]
    [InlineData(Dificuldade.Hard, 60)]
    [InlineData(Dificuldade.Expert, 120)]
    public void XpExercicio_BateComAAncoraDoPlano(Dificuldade dificuldade, int xpEsperado)
    {
        Assert.Equal(xpEsperado, _tabela.XpExercicio(dificuldade));
    }

    [Fact]
    public void XpExercicio_DificuldadeInvalida_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _tabela.XpExercicio((Dificuldade)999));
    }
}
