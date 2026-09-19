using CodeQuest.Services.Regras;
using Xunit;

namespace CodeQuest.Tests;

public class CalculadoraNivelTopicoTests
{
    private readonly CalculadoraNivelTopico _calc = new();

    [Fact]
    public void Calcular_SemNenhumDado_RetornaZero()
    {
        Assert.Equal(0, _calc.Calcular(notaUltimoTeste: null, notasExercicios30Dias: []));
    }

    [Fact]
    public void Calcular_SoTeste_UsaANotaDoTesteDireto()
    {
        Assert.Equal(80, _calc.Calcular(notaUltimoTeste: 80, notasExercicios30Dias: []));
    }

    [Fact]
    public void Calcular_SoExercicios_MediaSimples()
    {
        Assert.Equal(60, _calc.Calcular(notaUltimoTeste: null, notasExercicios30Dias: [40, 80]));
    }

    [Fact]
    public void Calcular_TesteEExercicios_TestePesaTresVezesMais()
    {
        // Teste 90 (peso 3) + exercício 30 (peso 1) = (90*3 + 30) / 4 = 75
        Assert.Equal(75, _calc.Calcular(notaUltimoTeste: 90, notasExercicios30Dias: [30]));
    }

    [Fact]
    public void Calcular_ResultadoSempreDentroDeZeroA100()
    {
        var resultado = _calc.Calcular(notaUltimoTeste: 100, notasExercicios30Dias: [100, 100]);

        Assert.InRange(resultado, 0, 100);
    }
}
