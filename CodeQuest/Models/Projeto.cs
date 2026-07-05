namespace CodeQuest.Models;

/// <summary>Projeto prático que pode receber sessões de foco.</summary>
public class Projeto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public StatusProjeto Status { get; set; } = StatusProjeto.Ativo;

    // Navegação
    public ICollection<SessaoFoco> SessoesFoco { get; set; } = new List<SessaoFoco>();
}
