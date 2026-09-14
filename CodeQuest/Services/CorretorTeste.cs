using System.Linq;
using CodeQuest.Services.Ai;

namespace CodeQuest.Services;

/// <summary>Resultado da correção de um teste/boss fight: nota + detalhe por questão.</summary>
public sealed record CorrecaoTeste(
    int Nota,
    IReadOnlyList<bool> Acertos,
    IReadOnlyList<string> Explicacoes,
    IReadOnlyList<string> Gaps);

/// <summary>
/// Corrige testes de avaliação (RF17) e boss fights (RN08): ambos são baterias de múltipla
/// escolha geradas pela IA (<see cref="TesteIa"/>), então a correção é idêntica — só o que
/// acontece com a nota depois (nível do tópico vs. conclusão de módulo) difere por serviço.
/// </summary>
public static class CorretorTeste
{
    public static CorrecaoTeste Corrigir(TesteIa teste, IReadOnlyList<int> respostas)
    {
        var acertos = new List<bool>();
        var explicacoes = new List<string>();
        var gaps = new List<string>();

        for (var i = 0; i < teste.Questoes.Count; i++)
        {
            var questao = teste.Questoes[i];
            var indiceEscolhido = i < respostas.Count ? respostas[i] : -1;
            var correta = indiceEscolhido >= 0 && indiceEscolhido < questao.Alternativas.Count
                && questao.Alternativas[indiceEscolhido].Correta;

            acertos.Add(correta);
            var alternativaCorreta = questao.Alternativas.First(a => a.Correta);
            explicacoes.Add(alternativaCorreta.Explicacao);
            if (!correta) gaps.Add(questao.Conceito);
        }

        var nota = teste.Questoes.Count == 0 ? 0 : (int)Math.Round(100.0 * acertos.Count(a => a) / teste.Questoes.Count);
        return new CorrecaoTeste(nota, acertos, explicacoes, gaps);
    }
}
