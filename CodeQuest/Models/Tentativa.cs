namespace CodeQuest.Models;

/// <summary>Resposta do usuário a um exercício, corrigida pela IA (RF15).</summary>
public class Tentativa
{
    public int Id { get; set; }

    public int ExercicioId { get; set; }
    public Exercicio Exercicio { get; set; } = null!;

    /// <summary>Texto ou código enviado pelo usuário.</summary>
    public string Resposta { get; set; } = string.Empty;

    /// <summary>Nota 0–100. Aprovado ≥ 70 (RN06).</summary>
    public int Nota { get; set; }

    /// <summary>Feedback da correção da IA.</summary>
    public string? FeedbackJson { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;

    /// <summary>Revisão agendada quando a tentativa reprova (RN06/RF16).</summary>
    public Revisao? Revisao { get; set; }
}
