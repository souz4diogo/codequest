namespace CodeQuest.Models;

/// <summary>Associação N:N entre missão e exercício. PK composta (MissaoId, ExercicioId).</summary>
public class MissaoExercicio
{
    public int MissaoId { get; set; }
    public Missao Missao { get; set; } = null!;

    public int ExercicioId { get; set; }
    public Exercicio Exercicio { get; set; } = null!;
}
