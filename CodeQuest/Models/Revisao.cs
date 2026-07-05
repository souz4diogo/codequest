namespace CodeQuest.Models;

/// <summary>Item da fila de repetição espaçada: +1, +3, +7 dias (RN06/RF16).</summary>
public class Revisao
{
    public int Id { get; set; }

    /// <summary>Tentativa que reprovou.</summary>
    public int TentativaId { get; set; }
    public Tentativa Tentativa { get; set; } = null!;

    public DateOnly AgendadaPara { get; set; }

    /// <summary>Ciclo da revisão: 1, 2 ou 3.</summary>
    public int Ciclo { get; set; }

    public bool Concluida { get; set; }
}
