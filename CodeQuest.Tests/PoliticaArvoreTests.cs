using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaArvoreTests
{
    private readonly PoliticaArvore _politica = new();

    [Fact]
    public void BossLiberado_SemTopicos_NaoLibera()
    {
        Assert.False(_politica.BossLiberado([]));
    }

    [Fact]
    public void BossLiberado_TodosOsTopicosNoMinimo_Libera()
    {
        Assert.True(_politica.BossLiberado([60, 60, 100]));
    }

    [Fact]
    public void BossLiberado_UmTopicoAbaixoDoMinimo_NaoLibera()
    {
        Assert.False(_politica.BossLiberado([60, 59, 100]));
    }

    [Theory]
    [InlineData(69, false)]
    [InlineData(70, true)] // limiar exato aprova
    public void BossAprovado_LimiarDe70(int nota, bool esperado)
    {
        Assert.Equal(esperado, _politica.BossAprovado(nota));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void BossAprovado_NotaForaDaFaixa_Lanca(int nota)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _politica.BossAprovado(nota));
    }
}
