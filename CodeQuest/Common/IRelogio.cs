namespace CodeQuest.Common;

/// <summary>
/// Abstração do tempo (DIP). Permite que as regras de streak/revisão sejam testáveis
/// sem depender de DateTime.Now — basta injetar um relógio falso nos testes.
/// </summary>
public interface IRelogio
{
    DateTime Agora { get; }
    DateOnly Hoje { get; }
}

/// <summary>Implementação real baseada no relógio do sistema (horário local do usuário).</summary>
public sealed class RelogioSistema : IRelogio
{
    public DateTime Agora => DateTime.Now;
    public DateOnly Hoje => DateOnly.FromDateTime(DateTime.Now);
}
