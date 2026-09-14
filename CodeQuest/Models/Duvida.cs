namespace CodeQuest.Models;

/// <summary>Dúvida enviada ao mentor IA, salva e marcável para revisão (RF22, RF23).</summary>
public class Duvida
{
    public int Id { get; set; }

    /// <summary>Player dono da dúvida (multi-usuário).</summary>
    public int PlayerId { get; set; }
    public Player? Player { get; set; }

    public int? TopicoId { get; set; }
    public Topico? Topico { get; set; }

    public string Pergunta { get; set; } = string.Empty;
    public string? RespostaJson { get; set; }

    public bool MarcadaParaRevisao { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
