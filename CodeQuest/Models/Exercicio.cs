namespace CodeQuest.Models;

/// <summary>Exercício gerado pela IA e persistido integralmente (RF14, RF18).</summary>
public class Exercicio
{
    public int Id { get; set; }

    public int TopicoId { get; set; }
    public Topico Topico { get; set; } = null!;

    public Dificuldade Dificuldade { get; set; }
    public FormatoExercicio Formato { get; set; }

    /// <summary>Enunciado no schema fixo da IA.</summary>
    public string EnunciadoJson { get; set; } = string.Empty;

    /// <summary>Critérios de correção / gabarito.</summary>
    public string GabaritoJson { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<Tentativa> Tentativas { get; set; } = new List<Tentativa>();
    public ICollection<MissaoExercicio> Missoes { get; set; } = new List<MissaoExercicio>();
}
