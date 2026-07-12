namespace CodeQuest.Models;

// Enums do domínio (seção 2.3 do documento de requisitos).
// Persistidos como TEXT no Postgres via HasConversion<string>() — legíveis no banco
// e imunes a reordenação dos membros.

public enum Dificuldade
{
    Easy,
    Medium,
    Hard,
    Expert
}

public enum TipoMissao
{
    Diaria,
    Semanal,
    Manual,
    SugeridaIA,
    Boss
}

public enum StatusMissao
{
    Pendente,
    EmAndamento,
    Concluida,
    Expirada
}

/// <summary>
/// Formatos de exercício (seção 8 do motor-ia: "treinar como se trabalha" — o dia a dia de
/// junior é ler, debugar e mexer em código dos outros, não só escrever do zero).
/// </summary>
public enum FormatoExercicio
{
    /// <summary>Teoria rápida: 4 alternativas com distratores de erros comuns.</summary>
    MultiplaEscolha,

    /// <summary>Implementar a partir do enunciado (escrita).</summary>
    Codigo,

    /// <summary>Código com 1–3 bugs plantados + comportamento esperado vs. atual; ache e corrija.</summary>
    Debug,

    /// <summary>Código feio que FUNCIONA; melhore sem quebrar os casos de teste.</summary>
    Refactor,

    /// <summary>"PR" simulado com problemas plantados; aponte-os como num review real.</summary>
    CodeReview,

    /// <summary>Código sem comentários; explique o que faz e preveja a saída.</summary>
    LeituraDeCodigo
}

public enum StatusModulo
{
    Bloqueado,
    Liberado,
    Concluido
}

// Não está na lista original de enums, mas a tabela Projeto usa os mesmos valores como TEXT.
public enum StatusProjeto
{
    Ativo,
    Pausado,
    Concluido
}
