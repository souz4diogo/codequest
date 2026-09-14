namespace CodeQuest.Models;

/// <summary>
/// Teste de avaliação (RF17) por tópico, ou boss fight (RF12/RN08) por módulo — exatamente
/// um dos dois. Atualiza o nível do tópico (RN07) e registra gaps; no boss, decide a nota
/// do módulo (<see cref="Modulo.NotaBoss"/>).
/// </summary>
public class Teste
{
    public int Id { get; set; }

    public int? TopicoId { get; set; }
    public Topico? Topico { get; set; }

    public int? ModuloId { get; set; }
    public Modulo? Modulo { get; set; }

    public Dificuldade Dificuldade { get; set; }

    /// <summary>Questões + gabarito.</summary>
    public string QuestoesJson { get; set; } = string.Empty;

    public string? RespostasJson { get; set; }

    /// <summary>Nota 0–100, null enquanto não corrigido.</summary>
    public int? Nota { get; set; }

    /// <summary>Lista de conceitos fracos identificados.</summary>
    public string? GapsJson { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
