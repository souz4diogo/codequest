using CodeQuest.Models;

namespace CodeQuest.Services.Ai;

/// <summary>
/// Templates de prompt e schemas de resposta, centralizados (seções 2, 4 e 5 do
/// codequest-motor-ia.md). Nenhum prompt vive em service ou controller.
/// </summary>
public static class Prompts
{
    /// <summary>Temperatura da geração de exercício: alta = variedade (seção 3 do motor-ia).</summary>
    public const double TemperaturaGeracao = 0.9;

    /// <summary>Temperatura da correção: baixa = notas consistentes (seção 5 do motor-ia).</summary>
    public const double TemperaturaCorrecao = 0.2;

    /// <summary>Rubrica de dificuldade (seção 2) — sem ela, "hard" não significa nada pra IA.</summary>
    public const string RubricaDificuldade = """
        EASY: um único conceito, aplicado diretamente. Código de 3–8 linhas. Sem pegadinhas. Objetivo: construir confiança.
        MEDIUM: combina 2 conceitos do tópico OU o conceito com uma variação não óbvia. 8–20 linhas. Exige entender, não decorar.
        HARD: cenário realista com decisão de design. Combina o tópico com conhecimento prévio de outros módulos. Pode ter pegadinha idiomática (ex.: deferred execution em LINQ). 20–40 linhas.
        EXPERT: problema aberto estilo entrevista técnica. Múltiplas soluções válidas com trade-offs (performance, legibilidade). O gabarito deve discutir alternativas.
        """;

    /// <summary>System prompt fixo da GERAÇÃO de exercício (seção 4), com a rubrica embutida.</summary>
    public static readonly string SystemGeracao = $"""
        Você é um professor sênior de C#/.NET criando exercícios de programação em português do Brasil.
        Regras:
        - Siga RIGOROSAMENTE a rubrica de dificuldade fornecida.
        - O exercício deve usar o CENÁRIO fornecido (contexto de mundo real ou de jogo). Nunca crie exercícios abstratos tipo "some os pares da lista".
        - NÃO repita nenhum dos exercícios recentes listados.
        - Código idiomático e moderno (.NET 8+): nullable habilitado, expressões, sem Console.ReadLine desnecessário.
        - Para formato "Codigo": o enunciado inclui código de partida (starter) que compila, e o gabarito inclui a solução completa comentada e 3 casos de teste (entrada → saída esperada).
        - Para "MultiplaEscolha": 4 alternativas, distratores plausíveis baseados em ERROS COMUNS reais do conceito, e explicação de por que cada alternativa está certa/errada.
        - Para "Debug": codigoApresentado traz um código com 1 a 3 bugs plantados; comportamentoEsperado descreve o comportamento esperado vs. o atual (inclua stack trace quando fizer sentido); problemasPlantados lista cada bug (trecho, tipo, explicação) e solucao traz o código corrigido.
        - Para "Refactor": codigoApresentado traz um código feio que FUNCIONA; casosDeTeste lista os casos que devem continuar passando (serão mostrados ao aluno como contrato); problemasPlantados lista os cheiros de código presentes; solucao traz a versão refatorada explicando cada melhoria.
        - Para "CodeReview": codigoApresentado simula um pull request com 2 a 4 problemas plantados (bug sutil, má prática, naming ruim); problemasPlantados é o gabarito — um item por problema, escrito como comentário de code review real; não corrija o código na solucao.
        - Para "LeituraDeCodigo": codigoApresentado traz um código sem comentários; o enunciado pede para explicar o que ele faz e prever a saída; solucao traz a explicação e a saída corretas.

        RUBRICA DE DIFICULDADE:
        {RubricaDificuldade}
        """;

    /// <summary>Critérios de correção comuns a todos os formatos (seção 5 do motor-ia).</summary>
    private const string SystemCorrecaoBase = """
        Você é um corretor rigoroso porém encorajador de exercícios de C#. Avalie a resposta do aluno contra o gabarito e os critérios.
        - Nota 0–100. Aprovado = nota >= 70.
        - Correção conceitual importa mais que sintaxe idêntica ao gabarito: soluções diferentes mas corretas merecem nota alta.
        - Aponte no máximo 3 problemas, do mais grave ao menos grave, cada um com o trecho exato e a correção.
        - Termine com 1 elogio específico sobre algo que o aluno fez bem (se houver).
        - Liste os conceitos que o aluno demonstrou NÃO dominar (para o sistema agendar revisão).
        """;

    /// <summary>
    /// System prompt da CORREÇÃO com o critério específico do formato (seção 8: cada formato
    /// avalia uma habilidade diferente). Usar com <see cref="TemperaturaCorrecao"/>.
    /// </summary>
    public static string SystemCorrecaoPara(FormatoExercicio formato) => $"""
        {SystemCorrecaoBase}

        CRITÉRIO ESPECÍFICO DESTE FORMATO ({formato}):
        {CriterioCorrecao(formato)}
        """;

    private static string CriterioCorrecao(FormatoExercicio formato) => formato switch
    {
        FormatoExercicio.MultiplaEscolha =>
            "Compare a resposta com a alternativa correta do gabarito: correta = 100, incorreta = 0, sem nota parcial. Use as explicações das alternativas no feedback.",
        FormatoExercicio.Codigo =>
            "Avalie contra a solucao e os casosDeTeste do gabarito: correção funcional pesa mais que estilo.",
        FormatoExercicio.Debug =>
            "A nota é a proporção dos bugs de problemasPlantados que o aluno identificou E corrigiu (todos = 100). Correção que introduz bug novo ou viola o comportamentoEsperado desconta.",
        FormatoExercicio.Refactor =>
            "Reprove (nota < 70) se a refatoração quebrar qualquer um dos casosDeTeste do gabarito. Passando todos, a nota mede quantos dos problemasPlantados (cheiros de código) a melhoria de fato resolveu.",
        FormatoExercicio.CodeReview =>
            "A nota é a proporção dos problemasPlantados do gabarito que o aluno apontou no review (encontrou todos = 100). Apontar algo que não está plantado não desconta, mas também não pontua.",
        FormatoExercicio.LeituraDeCodigo =>
            "Compare a explicação e a previsão de saída do aluno com a solucao do gabarito: saída correta e explicação que demonstra entender o fluxo = nota alta; previsão de saída errada limita a nota a 50.",
        _ => throw new ArgumentOutOfRangeException(nameof(formato)),
    };

    /// <summary>User prompt da geração (seção 4): tópico + rubrica aplicada + histórico do aluno + cenário sorteado.</summary>
    public static string UserGeracao(
        string topico,
        string modulo,
        Dificuldade dificuldade,
        FormatoExercicio formato,
        int nivelAluno,
        IReadOnlyCollection<string> errosRecentes,
        IReadOnlyCollection<string> exerciciosRecentes,
        string cenario) => $"""
        TÓPICO: {topico} (módulo: {modulo})
        DIFICULDADE: {dificuldade}
        FORMATO: {formato}
        NÍVEL DO ALUNO NO TÓPICO: {nivelAluno}/100
        ERROS RECENTES DO ALUNO NESTE TÓPICO: {(errosRecentes.Count > 0 ? string.Join("; ", errosRecentes) : "nenhum registrado")}
        EXERCÍCIOS RECENTES (NÃO REPETIR): {(exerciciosRecentes.Count > 0 ? string.Join("; ", exerciciosRecentes) : "nenhum")}
        CENÁRIO OBRIGATÓRIO: {cenario}
        """;

    /// <summary>User prompt da correção (seção 5): exercício + gabarito + resposta do aluno.</summary>
    public static string UserCorrecao(string gabaritoJson, string resposta) => $"""
        EXERCÍCIO E GABARITO (JSON):
        {gabaritoJson}

        RESPOSTA DO ALUNO:
        {resposta}
        """;

    /// <summary>responseSchema da geração (seção 4 do motor-ia) — casa com <see cref="ExercicioIa"/>.</summary>
    public static readonly object ExercicioSchema = new
    {
        type = "object",
        properties = new
        {
            titulo = new { type = "string" },
            enunciado = new { type = "string", description = "Markdown, com cenário e requisitos claros" },
            codigoPartida = new { type = "string", description = "starter code (formato Codigo); vazio nos demais" },
            codigoApresentado = new
            {
                type = "string",
                description = "código mostrado ao aluno: com bugs plantados (Debug), funcional a melhorar (Refactor), PR a revisar (CodeReview), código a explicar (LeituraDeCodigo); vazio nos demais formatos",
            },
            comportamentoEsperado = new
            {
                type = "string",
                description = "Debug: comportamento esperado vs. comportamento atual observado, com stack trace quando fizer sentido; vazio nos demais formatos",
            },
            problemasPlantados = new
            {
                type = "array",
                description = "GABARITO de Debug/Refactor/CodeReview: cada problema plantado no código apresentado",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        trecho = new { type = "string" },
                        tipo = new { type = "string", description = "bug | má prática | naming | performance | design" },
                        explicacao = new { type = "string" },
                    },
                    required = new[] { "trecho", "tipo", "explicacao" },
                },
            },
            alternativas = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        texto = new { type = "string" },
                        correta = new { type = "boolean" },
                        explicacao = new { type = "string" },
                    },
                    required = new[] { "texto", "correta", "explicacao" },
                },
            },
            solucao = new { type = "string", description = "solução completa comentada; vazio se múltipla escolha" },
            casosDeTeste = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new { entrada = new { type = "string" }, saidaEsperada = new { type = "string" } },
                    required = new[] { "entrada", "saidaEsperada" },
                },
            },
            conceitosAvaliados = new { type = "array", items = new { type = "string" } },
            dicas = new { type = "array", items = new { type = "string" }, description = "3 dicas progressivas" },
        },
        required = new[] { "titulo", "enunciado", "conceitosAvaliados", "dicas" },
    };

    /// <summary>responseSchema da correção (seção 5 do motor-ia) — casa com <see cref="CorrecaoIa"/>.</summary>
    public static readonly object CorrecaoSchema = new
    {
        type = "object",
        properties = new
        {
            nota = new { type = "integer" },
            aprovado = new { type = "boolean" },
            problemas = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        trecho = new { type = "string" },
                        explicacao = new { type = "string" },
                        correcao = new { type = "string" },
                    },
                    required = new[] { "trecho", "explicacao", "correcao" },
                },
            },
            elogio = new { type = "string" },
            conceitosParaRevisar = new { type = "array", items = new { type = "string" } },
        },
        required = new[] { "nota", "aprovado", "conceitosParaRevisar" },
    };
}
