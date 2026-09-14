namespace CodeQuest.Services.Ai;

// Contratos EXATOS dos responseSchema (Prompts.ExercicioSchema / Prompts.CorrecaoSchema).
// A IA devolve JSON nessas formas e em nenhuma outra (RNF05: nunca parsear texto livre).
// Nada aqui carrega XP/gold — recompensa é sempre decisão do backend (RN09).

/// <summary>Alternativa de múltipla escolha, com o motivo de estar certa/errada.</summary>
public sealed record AlternativaIa(string Texto, bool Correta, string Explicacao);

/// <summary>Caso de teste: entrada → saída esperada. Gabarito no formato Código; contrato
/// PÚBLICO no Refactor (os casos que a melhoria não pode quebrar).</summary>
public sealed record CasoDeTesteIa(string Entrada, string SaidaEsperada);

/// <summary>
/// Problema plantado no código apresentado — gabarito de Debug (bugs), Refactor (cheiros de
/// código) e CodeReview (bug sutil, má prática, naming). No CodeReview, a nota é a proporção
/// destes que o aluno encontrou.
/// </summary>
public sealed record ProblemaPlantadoIa(string Trecho, string Tipo, string Explicacao);

/// <summary>Exercício completo gerado pela IA (enunciado + gabarito, seções 4 e 8 do motor-ia).</summary>
public sealed record ExercicioIa(
    string Titulo,
    string Enunciado,
    string? CodigoPartida,
    string? CodigoApresentado,
    string? ComportamentoEsperado,
    IReadOnlyList<AlternativaIa>? Alternativas,
    IReadOnlyList<ProblemaPlantadoIa>? ProblemasPlantados,
    string? Solucao,
    IReadOnlyList<CasoDeTesteIa>? CasosDeTeste,
    IReadOnlyList<string> ConceitosAvaliados,
    IReadOnlyList<string> Dicas);

/// <summary>Problema apontado na correção: trecho exato, explicação e como corrigir.</summary>
public sealed record ProblemaIa(string Trecho, string Explicacao, string Correcao);

/// <summary>Parecer da correção (seção 5 do motor-ia). A nota é da IA; aprovação e XP são do backend.</summary>
/// <summary>Resposta do mentor a uma dúvida (RF22), em Markdown.</summary>
public sealed record RespostaMentorIa(string Resposta);

/// <summary>Missão estruturada pela IA a partir de texto livre do aluno (RF10).</summary>
public sealed record MissaoSugeridaIa(string Titulo, string Descricao, string Esforco);

/// <summary>Uma questão de múltipla escolha de um teste de avaliação ou boss fight (RF17/RF12).</summary>
public sealed record QuestaoTesteIa(
    string Conceito,
    string Enunciado,
    IReadOnlyList<AlternativaIa> Alternativas);

/// <summary>Teste completo: 5–10 questões (RF17) ou o conjunto de um boss fight (RN08).</summary>
public sealed record TesteIa(IReadOnlyList<QuestaoTesteIa> Questoes);

public sealed record CorrecaoIa(
    int Nota,
    bool Aprovado,
    IReadOnlyList<ProblemaIa>? Problemas,
    string? Elogio,
    IReadOnlyList<string> ConceitosParaRevisar);
