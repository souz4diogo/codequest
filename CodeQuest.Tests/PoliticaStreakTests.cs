using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class PoliticaStreakTests
{
    private readonly PoliticaStreak _politica = new();
    private static readonly DateOnly Hoje = new(2026, 6, 15);

    [Fact]
    public void Avaliar_PrimeiraAtividadeDeTodas_ComecaStreakEm1()
    {
        var r = _politica.Avaliar(streakAtual: 0, ultimoDiaAtivo: null, Hoje, pocaoAtiva: false);

        Assert.Equal(1, r.NovoStreak);
        Assert.True(r.ContabilizouDia);
        Assert.False(r.ConsumiuPocao);
    }

    [Fact]
    public void Avaliar_SegundaAtividadeNoMesmoDia_NaoMudaNada()
    {
        var r = _politica.Avaliar(streakAtual: 3, ultimoDiaAtivo: Hoje, Hoje, pocaoAtiva: false);

        Assert.Equal(3, r.NovoStreak);
        Assert.False(r.ContabilizouDia);
    }

    [Fact]
    public void Avaliar_DiaConsecutivo_Incrementa()
    {
        var r = _politica.Avaliar(streakAtual: 3, ultimoDiaAtivo: Hoje.AddDays(-1), Hoje, pocaoAtiva: false);

        Assert.Equal(4, r.NovoStreak);
        Assert.True(r.ContabilizouDia);
        Assert.False(r.ConsumiuPocao);
    }

    [Fact]
    public void Avaliar_PerdeuUmDiaComPocaoAtiva_ProtegeEConsomePocao()
    {
        var r = _politica.Avaliar(streakAtual: 5, ultimoDiaAtivo: Hoje.AddDays(-2), Hoje, pocaoAtiva: true);

        Assert.Equal(6, r.NovoStreak);
        Assert.True(r.ContabilizouDia);
        Assert.True(r.ConsumiuPocao);
    }

    [Fact]
    public void Avaliar_PerdeuUmDiaSemPocao_ZeraERecomecaEm1()
    {
        var r = _politica.Avaliar(streakAtual: 5, ultimoDiaAtivo: Hoje.AddDays(-2), Hoje, pocaoAtiva: false);

        Assert.Equal(1, r.NovoStreak);
        Assert.False(r.ConsumiuPocao);
    }

    [Fact]
    public void Avaliar_PerdeuVariosDiasMesmoComPocao_PocaoSoProtege1Dia_Zera()
    {
        var r = _politica.Avaliar(streakAtual: 5, ultimoDiaAtivo: Hoje.AddDays(-5), Hoje, pocaoAtiva: true);

        Assert.Equal(1, r.NovoStreak);
        Assert.False(r.ConsumiuPocao);
    }
}
