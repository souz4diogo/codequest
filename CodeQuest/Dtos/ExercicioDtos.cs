using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Ai;

namespace CodeQuest.Dtos;

/// <summary>Pedido de geração de exercício (RF14). Enums viajam como string.</summary>
public sealed record GerarExercicioRequest(int TopicoId, Dificuldade Dificuldade, FormatoExercicio Formato);

/// <summary>
/// Exercício exposto ao front: apenas o enunciado público (título, texto, starter, alternativas
/// sem a correta, dicas). O gabarito NUNCA sai do backend antes da correção.
/// </summary>
public sealed record ExercicioDto(
    int Id,
    int TopicoId,
    Dificuldade Dificuldade,
    FormatoExercicio Formato,
    string EnunciadoJson,
    DateTime CriadoEm);

/// <summary>Resposta do aluno a um exercício (RF15): texto ou código.</summary>
public sealed record ResponderExercicioRequest(string Resposta);

/// <summary>
/// Parecer devolvido após a correção: nota/feedback da IA + as decisões do backend —
/// XP da tabela (RN09) quando aprovado, revisão agendada (RN06) quando reprovado.
/// </summary>
public sealed record CorrecaoDto(
    int TentativaId,
    int Nota,
    bool Aprovado,
    IReadOnlyList<ProblemaIa> Problemas,
    string? Elogio,
    IReadOnlyList<string> ConceitosParaRevisar,
    ResultadoXpDto? Recompensa,
    DateOnly? RevisaoAgendadaPara);

/// <summary>Mapeamentos de exercício → DTO.</summary>
public static class MapeamentosExercicioDto
{
    public static ExercicioDto ParaDto(this Exercicio e) =>
        new(e.Id, e.TopicoId, e.Dificuldade, e.Formato, e.EnunciadoJson, e.CriadoEm);

    public static CorrecaoDto ParaDto(this ResultadoTentativa r) =>
        new(r.Tentativa.Id,
            r.Tentativa.Nota,
            r.Aprovado,
            r.Correcao.Problemas ?? [],
            r.Correcao.Elogio,
            r.Correcao.ConceitosParaRevisar,
            r.Recompensa?.ParaDto(),
            r.RevisaoAgendadaPara);
}
