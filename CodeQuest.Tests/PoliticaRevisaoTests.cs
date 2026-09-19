using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaRevisaoTests
{
    private readonly PoliticaRevisao _politica = new();
    private static readonly DateOnly Hoje = new(2026, 6, 15);

    [Theory]
    [InlineData(69, false)]
    [InlineData(70, true)] // limiar exato aprova (RN06: nota >= 70)
    [InlineData(100, true)]
    public void Aprovado_LimiarDe70(int nota, bool esperado)
    {
        Assert.Equal(esperado, _politica.Aprovado(nota));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Aprovado_NotaForaDaFaixa_Lanca(int nota)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _politica.Aprovado(nota));
    }

    [Fact]
    public void AposReprovar_PrimeiraReprovacao_AgendaPara1Dia()
    {
        var r = _politica.AposReprovar(cicloAtual: 0, Hoje);

        Assert.NotNull(r);
        Assert.Equal(1, r!.Value.Ciclo);
        Assert.Equal(Hoje.AddDays(1), r.Value.AgendadaPara);
    }

    [Fact]
    public void AposReprovar_SegundaReprovacao_AgendaPara3Dias()
    {
        var r = _politica.AposReprovar(cicloAtual: 1, Hoje);

        Assert.Equal(2, r!.Value.Ciclo);
        Assert.Equal(Hoje.AddDays(3), r.Value.AgendadaPara);
    }

    [Fact]
    public void AposReprovar_TerceiraReprovacao_AgendaPara7Dias()
    {
        var r = _politica.AposReprovar(cicloAtual: 2, Hoje);

        Assert.Equal(3, r!.Value.Ciclo);
        Assert.Equal(Hoje.AddDays(7), r.Value.AgendadaPara);
    }

    [Fact]
    public void AposReprovar_CiclosEsgotados_SaiDaFila()
    {
        var r = _politica.AposReprovar(cicloAtual: 3, Hoje);

        Assert.Null(r);
    }

    [Fact]
    public void AposReprovar_CicloNegativo_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _politica.AposReprovar(-1, Hoje));
    }
}
