using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaXpConfortoTests
{
    private readonly PoliticaXpConforto _politica = new();

    [Theory]
    [InlineData(Dificuldade.Easy, 39, 1.0)] // abaixo do limiar: paga cheio ainda
    [InlineData(Dificuldade.Easy, 40, 0.50)] // começa a reduzir
    [InlineData(Dificuldade.Easy, 59, 0.50)]
    [InlineData(Dificuldade.Easy, 60, 0.10)] // quase zera
    [InlineData(Dificuldade.Medium, 59, 1.0)]
    [InlineData(Dificuldade.Medium, 60, 0.60)]
    [InlineData(Dificuldade.Hard, 100, 1.0)] // hard/expert sempre pagam cheio
    [InlineData(Dificuldade.Expert, 100, 1.0)]
    public void Fator_SegueATabelaDeConforto(Dificuldade dificuldade, int nivelTopico, double fatorEsperado)
    {
        Assert.Equal(fatorEsperado, _politica.Fator(dificuldade, nivelTopico), precision: 5);
    }

    [Fact]
    public void Ajustar_AplicaOFatorEArredonda()
    {
        // Easy, nível 60 -> fator 0.10; 15 XP * 0.10 = 1.5 -> arredonda pra 2 (AwayFromZero)
        Assert.Equal(2, _politica.Ajustar(15, Dificuldade.Easy, nivelTopico: 60));
    }

    [Fact]
    public void Ajustar_XpNegativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _politica.Ajustar(-1, Dificuldade.Easy, 0));
    }
}
