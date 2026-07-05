namespace CodeQuest.Models;

/// <summary>Missão do jogador (RF09–RF13). Estados em StatusMissao.</summary>
public class Missao
{
    public int Id { get; set; }

    public TipoMissao Tipo { get; set; }

    /// <summary>Tópico foco (opcional).</summary>
    public int? TopicoId { get; set; }
    public Topico? Topico { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    /// <summary>XP validado contra a tabela de recompensas/RN04 — nunca vem da IA (RN09).</summary>
    public int XpRecompensa { get; set; }

    /// <summary>Gold derivado do XP (RN02).</summary>
    public int GoldRecompensa { get; set; }

    public StatusMissao Status { get; set; } = StatusMissao.Pendente;

    /// <summary>Dia/semana alvo da missão.</summary>
    public DateOnly DataAlvo { get; set; }

    public DateTime? ConcluidaEm { get; set; }

    /// <summary>Prompt/resposta da IA que gerou a missão, se houver.</summary>
    public string? OrigemJson { get; set; }

    // Navegação
    public ICollection<MissaoExercicio> Exercicios { get; set; } = new List<MissaoExercicio>();
}
