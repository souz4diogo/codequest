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

/// <summary>
/// Implementação real baseada no relógio do sistema, em <b>UTC</b>. O Postgres persiste os
/// <c>DateTime</c> como <c>timestamptz</c> e só aceita <c>Kind=Utc</c> — usar horário local
/// estoura na escrita. Todos os defaults dos models (<c>DateTime.UtcNow</c>) seguem a mesma convenção.
/// </summary>
public sealed class RelogioSistema : IRelogio
{
    public DateTime Agora => DateTime.UtcNow;
    public DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);
}
