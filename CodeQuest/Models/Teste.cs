namespace CodeQuest.Models;

/// <summary>Teste de avaliação por tópico (RF17). Atualiza o nível do tópico e registra gaps.</summary>
public class Teste
{
    public int Id { get; set; }

    public int TopicoId { get; set; }
    public Topico Topico { get; set; } = null!;

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
