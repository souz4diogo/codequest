namespace CodeQuest.Models;

/// <summary>Pré-requisito N:N entre módulos. PK composta (ModuloId, RequerModuloId).</summary>
public class ModuloPrereq
{
    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; } = null!;

    public int RequerModuloId { get; set; }
    public Modulo RequerModulo { get; set; } = null!;
}
