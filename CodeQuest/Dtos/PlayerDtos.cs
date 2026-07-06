using CodeQuest.Models;
using CodeQuest.Services;
using CodeQuest.Services.Regras;

namespace CodeQuest.Dtos;

/// <summary>Estado do jogador exposto ao front (inclui o XP que falta para o próximo nível, calculado no backend).</summary>
public sealed record PlayerDto(
    int Id,
    string Nome,
    int Nivel,
    int XpTotal,
    int Gold,
    int StreakDias,
    bool PocaoStreakAtiva,
    int XpParaProximoNivel);

/// <summary>Entrada do endpoint de teste que credita XP (equivalente aos botões "+XP" do Dashboard Blazor).</summary>
public sealed record AdicionarXpRequest(int XpBase);

/// <summary>Resultado detalhado de creditar XP — espelha o <see cref="ResultadoXp"/> do domínio.</summary>
public sealed record ResultadoXpDto(
    int XpBase,
    int XpCreditado,
    int GoldGanho,
    double Multiplicador,
    bool SubiuNivel,
    int NivelAtual,
    int StreakAtual);

/// <summary>Mapeamentos entidade/registro de domínio → DTO. Mantém os controllers enxutos.</summary>
public static class MapeamentosDto
{
    public static PlayerDto ParaDto(this Player p, IReguladorNivel nivel) =>
        new(p.Id, p.Nome, p.Nivel, p.XpTotal, p.Gold, p.StreakDias, p.PocaoStreakAtiva,
            nivel.XpFaltandoParaProximoNivel(p.XpTotal));

    public static ResultadoXpDto ParaDto(this ResultadoXp r) =>
        new(r.XpBase, r.XpCreditado, r.GoldGanho, r.Multiplicador, r.SubiuNivel, r.NivelAtual, r.StreakAtual);
}
