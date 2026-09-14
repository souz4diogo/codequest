using CodeQuest.Services;
using CodeQuest.Services.Ai;
using Xunit;

namespace CodeQuest.Tests;

public class CorretorTesteTests
{
    private static QuestaoTesteIa Questao(string conceito, int indiceCorreto, int quantidadeAlternativas = 4)
    {
        var alternativas = new List<AlternativaIa>();
        for (var i = 0; i < quantidadeAlternativas; i++)
        {
            var correta = i == indiceCorreto;
            alternativas.Add(new AlternativaIa($"Alternativa {i}", correta, correta ? "Explicação correta" : "Explicação errada"));
        }

        return new QuestaoTesteIa(conceito, "Enunciado qualquer", alternativas);
    }

    [Fact]
    public void Corrigir_TesteVazio_RetornaNotaZeroSemAcertos()
    {
        var teste = new TesteIa(new List<QuestaoTesteIa>());

        var resultado = CorretorTeste.Corrigir(teste, new List<int>());

        Assert.Equal(0, resultado.Nota);
        Assert.Empty(resultado.Acertos);
        Assert.Empty(resultado.Explicacoes);
        Assert.Empty(resultado.Gaps);
    }

    [Fact]
    public void Corrigir_TodasRespostasCorretas_Retorna100EExplicacoesCorretas()
    {
        var teste = new TesteIa(new List<QuestaoTesteIa>
        {
            Questao("Herança", 0),
            Questao("Polimorfismo", 2),
        });

        var resultado = CorretorTeste.Corrigir(teste, new List<int> { 0, 2 });

        Assert.Equal(100, resultado.Nota);
        Assert.Equal(new[] { true, true }, resultado.Acertos);
        Assert.All(resultado.Explicacoes, e => Assert.Equal("Explicação correta", e));
        Assert.Empty(resultado.Gaps);
    }

    [Fact]
    public void Corrigir_TodasRespostasErradas_Retorna0EAcumulaGapsPorConceito()
    {
        var teste = new TesteIa(new List<QuestaoTesteIa>
        {
            Questao("Herança", 0),
            Questao("Polimorfismo", 2),
        });

        var resultado = CorretorTeste.Corrigir(teste, new List<int> { 1, 1 });

        Assert.Equal(0, resultado.Nota);
        Assert.Equal(new[] { false, false }, resultado.Acertos);
        Assert.Equal(new[] { "Herança", "Polimorfismo" }, resultado.Gaps);
    }

    [Fact]
    public void Corrigir_RespostasParciais_ArredondaNotaCorretamente()
    {
        var teste = new TesteIa(new List<QuestaoTesteIa>
        {
            Questao("A", 0),
            Questao("B", 0),
            Questao("C", 0),
        });

        // 1 de 3 corretas = 33,33...% -> arredonda para 33
        var resultado = CorretorTeste.Corrigir(teste, new List<int> { 0, 1, 1 });

        Assert.Equal(33, resultado.Nota);
        Assert.Equal(new[] { true, false, false }, resultado.Acertos);
        Assert.Equal(new[] { "B", "C" }, resultado.Gaps);
    }

    [Fact]
    public void Corrigir_ListaDeRespostasMenorQueQuestoes_TrataFaltantesComoErradas()
    {
        var teste = new TesteIa(new List<QuestaoTesteIa>
        {
            Questao("A", 0),
            Questao("B", 1),
        });

        // Só respondeu a primeira questão
        var resultado = CorretorTeste.Corrigir(teste, new List<int> { 0 });

        Assert.Equal(50, resultado.Nota);
        Assert.Equal(new[] { true, false }, resultado.Acertos);
        Assert.Equal(new[] { "B" }, resultado.Gaps);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void Corrigir_IndiceDeRespostaForaDoRange_NaoLancaETrataComoErrada(int indiceInvalido)
    {
        var teste = new TesteIa(new List<QuestaoTesteIa> { Questao("A", 0) });

        var resultado = CorretorTeste.Corrigir(teste, new List<int> { indiceInvalido });

        Assert.False(resultado.Acertos[0]);
        Assert.Equal(0, resultado.Nota);
    }
}
