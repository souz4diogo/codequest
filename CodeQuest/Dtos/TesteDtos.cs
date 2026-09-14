using CodeQuest.Common;
using System.Linq;
using System.Text.Json;
using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Ai;

namespace CodeQuest.Dtos;

/// <summary>Pedido de teste de avaliação por tópico (RF17).</summary>
public sealed record IniciarTesteRequest(int TopicoId, Dificuldade Dificuldade);

/// <summary>Questão exposta ao aluno: sem indicar qual alternativa é a correta.</summary>
public sealed record QuestaoPublicaDto(string Enunciado, IReadOnlyList<string> Alternativas);

/// <summary>Teste/boss fight gerado, pronto para responder.</summary>
public sealed record TesteDto(int Id, int? TopicoId, int? ModuloId, Dificuldade Dificuldade,
    IReadOnlyList<QuestaoPublicaDto> Questoes, DateTime Data);

/// <summary>Respostas do aluno: índice da alternativa escolhida em cada questão, na ordem.</summary>
public sealed record ResponderTesteRequest(IReadOnlyList<int> Respostas);

/// <summary>Correção de um teste de tópico (RF17): nota + gaps para revisão.</summary>
public sealed record CorrecaoTesteDto(int TesteId, int Nota, IReadOnlyList<bool> Acertos,
    IReadOnlyList<string> Explicacoes, IReadOnlyList<string> Gaps);

/// <summary>Correção de um boss fight (RN08): nota, aprovação e módulos recém-liberados.</summary>
public sealed record CorrecaoBossDto(int TesteId, int Nota, IReadOnlyList<bool> Acertos,
    IReadOnlyList<string> Explicacoes, bool Aprovado, IReadOnlyList<string> ModulosLiberados);

public static class MapeamentosTesteDto
{

    public static TesteDto ParaDto(this Teste t)
    {
        var gerado = JsonSerializer.Deserialize<TesteIa>(t.QuestoesJson, JsonPadrao.Opcoes)!;
        var questoes = gerado.Questoes
            .Select(q => new QuestaoPublicaDto(q.Enunciado, q.Alternativas.Select(a => a.Texto).ToList()))
            .ToList();
        return new TesteDto(t.Id, t.TopicoId, t.ModuloId, t.Dificuldade, questoes, t.Data);
    }

    public static CorrecaoTesteDto ParaDto(this ResultadoTeste r) =>
        new(r.Teste.Id, r.Correcao.Nota, r.Correcao.Acertos, r.Correcao.Explicacoes, r.Correcao.Gaps);

    public static CorrecaoBossDto ParaDto(this ResultadoBoss r) =>
        new(r.Teste.Id, r.Correcao.Nota, r.Correcao.Acertos, r.Correcao.Explicacoes, r.Aprovado, r.ModulosLiberados);
}
