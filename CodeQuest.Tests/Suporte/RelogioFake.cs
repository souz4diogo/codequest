using CodeQuest.Common;

namespace CodeQuest.Tests.Suporte;

public sealed class RelogioFake : IRelogio
{
    public DateTime Agora { get; set; } = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    public DateOnly Hoje => DateOnly.FromDateTime(Agora);
}
