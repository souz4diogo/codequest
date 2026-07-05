namespace CodeQuest.Models;

// Enums do domínio (seção 2.3 do documento de requisitos).
// Persistidos como TEXT no SQLite via HasConversion<string>() — legíveis no banco
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

public enum FormatoExercicio
{
    MultiplaEscolha,
    Codigo
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
