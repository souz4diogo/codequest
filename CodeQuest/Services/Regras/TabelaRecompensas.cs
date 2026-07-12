using CodeQuest.Models;

namespace CodeQuest.Services.Regras;

/// <summary>
/// RN09 — tabela de recompensas do backend (plano, seção 2 "XP por atividade"). A IA gera
/// conteúdo e corrige, mas XP/gold saem SEMPRE daqui — nunca do texto da IA ("a IA sugere,
/// o sistema decide"). Exercício só pontua quando aprovado (RN06).
/// </summary>
public interface ITabelaRecompensas
{
    /// <summary>XP base de um exercício aprovado: easy 15, medium 30, hard 60, expert 120.</summary>
    int XpExercicio(Dificuldade dificuldade);
}

public sealed class TabelaRecompensas : ITabelaRecompensas
{
    public int XpExercicio(Dificuldade dificuldade) => dificuldade switch
    {
        Dificuldade.Easy => 15,
        Dificuldade.Medium => 30,
        Dificuldade.Hard => 60,
        Dificuldade.Expert => 120,
        _ => throw new ArgumentOutOfRangeException(nameof(dificuldade)),
    };
}
