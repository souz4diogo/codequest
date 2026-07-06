namespace CodeQuest.Models;

/// <summary>
/// Perfil de jogo de um usuário (relação 1:1 com <see cref="Usuario"/>). Regras RN01–RN05.
/// A entidade é apenas um contêiner de estado; toda a lógica de XP/nível/gold/streak
/// vive nos serviços (ver GameService), mantendo a classe fiel ao SRP.
/// </summary>
public class Player
{
    public int Id { get; set; }

    /// <summary>Dono deste perfil (1:1). Todo Player pertence a exatamente um usuário.</summary>
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>XP acumulado. Nunca decrementa (RN01).</summary>
    public int XpTotal { get; set; }

    /// <summary>Nível derivado do XP, cacheado por performance (RN01).</summary>
    public int Nivel { get; set; } = 1;

    /// <summary>Moeda do jogo. Nunca fica negativo (CHECK no banco).</summary>
    public int Gold { get; set; }

    public int StreakDias { get; set; }

    /// <summary>Último dia com atividade pontuável (RN05).</summary>
    public DateOnly? UltimoDiaAtivo { get; set; }

    /// <summary>Poção que protege a perda de 1 dia de streak (RF21/RN05).</summary>
    public bool PocaoStreakAtiva { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<Missao> Missoes { get; set; } = new List<Missao>();
    public ICollection<CompraLoja> Compras { get; set; } = new List<CompraLoja>();
    public ICollection<SessaoFoco> SessoesFoco { get; set; } = new List<SessaoFoco>();
}
