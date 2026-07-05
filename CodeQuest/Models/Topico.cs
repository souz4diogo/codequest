namespace CodeQuest.Models;

/// <summary>Tópico de estudo dentro de um módulo (RF05, RF07).</summary>
public class Topico
{
    public int Id { get; set; }

    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;

    /// <summary>Nível estimado 0–100, atualizado por testes/exercícios (RN07).</summary>
    public int NivelEstimado { get; set; }

    /// <summary>1 alta, 2 normal, 3 baixa.</summary>
    public int Prioridade { get; set; } = 2;

    public bool Arquivado { get; set; }

    // Navegação
    public ICollection<Exercicio> Exercicios { get; set; } = new List<Exercicio>();
    public ICollection<Teste> Testes { get; set; } = new List<Teste>();
    public ICollection<Duvida> Duvidas { get; set; } = new List<Duvida>();
}
