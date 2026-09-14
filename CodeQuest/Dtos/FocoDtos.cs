using CodeQuest.Models;

namespace CodeQuest.Dtos;

/// <summary>Registro de uma sessão Pomodoro encerrada (RF19).</summary>
public sealed record RegistrarFocoRequest(int TopicoId, int MinutosPlanejados, int MinutosReais);

public sealed record SessaoFocoDto(
    int Id,
    int TopicoId,
    int MinutosPlanejados,
    int MinutosReais,
    int XpGanho,
    bool Completou,
    DateTime Data);

public static class MapeamentosFocoDto
{
    public static SessaoFocoDto ParaDto(this SessaoFoco s) =>
        new(s.Id, s.TopicoId!.Value, s.MinutosPlanejados, s.MinutosReais, s.XpGanho,
            s.MinutosReais >= s.MinutosPlanejados, s.Data);
}
