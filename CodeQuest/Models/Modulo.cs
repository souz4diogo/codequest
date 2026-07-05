namespace CodeQuest.Models;

/// <summary>Área da árvore de habilidades (RF06). Seed com os 10 módulos do plano.</summary>
public class Modulo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    /// <summary>Posição na árvore.</summary>
    public int Ordem { get; set; }

    public StatusModulo Status { get; set; } = StatusModulo.Bloqueado;

    /// <summary>Nota do boss fight (0–100), null enquanto não enfrentado (RN08).</summary>
    public int? NotaBoss { get; set; }

    // Navegação
    public ICollection<Topico> Topicos { get; set; } = new List<Topico>();

    /// <summary>Módulos que este módulo exige como pré-requisito.</summary>
    public ICollection<ModuloPrereq> PreRequisitos { get; set; } = new List<ModuloPrereq>();
}
