namespace CodeQuest.Models;

/// <summary>
/// Sessão Pomodoro (RF19). XP só é creditado se MinutosReais ≥ MinutosPlanejados.
/// Vinculada a um projeto OU a um tópico.
/// </summary>
public class SessaoFoco
{
    public int Id { get; set; }

    public int? ProjetoId { get; set; }
    public Projeto? Projeto { get; set; }

    public int? TopicoId { get; set; }
    public Topico? Topico { get; set; }

    public int MinutosPlanejados { get; set; }
    public int MinutosReais { get; set; }
    public int XpGanho { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
