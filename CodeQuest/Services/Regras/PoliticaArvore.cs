namespace CodeQuest.Services.Regras;

/// <summary>
/// RN08 — Progressão na árvore de habilidades: o boss fight do módulo é liberado quando
/// todos os tópicos ativos estão com nível ≥ 60; o módulo é concluído com nota ≥ 70 no boss,
/// o que libera os módulos que dependiam dele (RF06/RF12). Função pura — o serviço de
/// aplicação (boss fights, Fase 4 do roadmap) consulta os tópicos e atualiza o Status.
/// </summary>
public interface IPoliticaArvore
{
    /// <summary>Se o boss do módulo está liberado, dado o nível dos tópicos ativos (não arquivados).</summary>
    bool BossLiberado(IReadOnlyCollection<int> niveisTopicosAtivos);

    /// <summary>Se a nota do boss conclui o módulo (RN08).</summary>
    bool BossAprovado(int notaBoss);
}

public sealed class PoliticaArvore : IPoliticaArvore
{
    public const int NivelMinimoTopico = 60;
    public const int NotaMinimaBoss = 70;

    public bool BossLiberado(IReadOnlyCollection<int> niveisTopicosAtivos) =>
        niveisTopicosAtivos.Count > 0 && niveisTopicosAtivos.All(n => n >= NivelMinimoTopico);

    public bool BossAprovado(int notaBoss)
    {
        if (notaBoss is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(notaBoss));
        return notaBoss >= NotaMinimaBoss;
    }
}
