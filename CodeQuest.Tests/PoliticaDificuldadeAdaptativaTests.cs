using CodeQuest.Models;
using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaDificuldadeAdaptativaTests
{
    private readonly PoliticaDificuldadeAdaptativa _politica = new();

    [Fact]
    public void Recomendar_SemHistorico_ComecaEmEasy()
    {
        var r = _politica.Recomendar(Dificuldade.Expert, taxaAcerto: 1.0, tentativasRecentes: 0);

        Assert.Equal(Dificuldade.Easy, r);
    }

    [Fact]
    public void Recomendar_AcertoAcimaDe85Porcento_Sobe()
    {
        var r = _politica.Recomendar(Dificuldade.Easy, taxaAcerto: 0.90, tentativasRecentes: 10);

        Assert.Equal(Dificuldade.Medium, r);
    }

    [Fact]
    public void Recomendar_JaNoExpertEAcertandoMuito_NaoUltrapassaExpert()
    {
        var r = _politica.Recomendar(Dificuldade.Expert, taxaAcerto: 1.0, tentativasRecentes: 10);

        Assert.Equal(Dificuldade.Expert, r);
    }

    [Fact]
    public void Recomendar_AcertoAbaixoDe50Porcento_Desce()
    {
        var r = _politica.Recomendar(Dificuldade.Medium, taxaAcerto: 0.30, tentativasRecentes: 10);

        Assert.Equal(Dificuldade.Easy, r);
    }

    [Fact]
    public void Recomendar_JaNoEasyEErrandoMuito_NaoDesceMaisQueEasy()
    {
        var r = _politica.Recomendar(Dificuldade.Easy, taxaAcerto: 0.10, tentativasRecentes: 10);

        Assert.Equal(Dificuldade.Easy, r);
    }

    [Theory]
    [InlineData(0.60)]
    [InlineData(0.85)] // limiar exato de subida NÃO sobe (regra é "> 0.85")
    [InlineData(0.50)] // limiar exato de descida NÃO desce (regra é "< 0.50")
    public void Recomendar_ZonaDeAprendizado_MantemADificuldadeAtual(double taxaAcerto)
    {
        var r = _politica.Recomendar(Dificuldade.Medium, taxaAcerto, tentativasRecentes: 10);

        Assert.Equal(Dificuldade.Medium, r);
    }

    [Fact]
    public void Recomendar_TentativasNegativas_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _politica.Recomendar(Dificuldade.Easy, 0.5, -1));
    }
}
