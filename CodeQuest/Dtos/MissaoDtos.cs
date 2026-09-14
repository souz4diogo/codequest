using CodeQuest.Models;
using CodeQuest.Services.Regras;

namespace CodeQuest.Dtos;

/// <summary>Missão exposta ao front. Enums viajam como string (ver JsonStringEnumConverter no Program).</summary>
public sealed record MissaoDto(
    int Id,
    string Titulo,
    string Descricao,
    TipoMissao Tipo,
    StatusMissao Status,
    int XpRecompensa,
    int GoldRecompensa,
    DateOnly DataAlvo,
    DateTime? ConcluidaEm);

/// <summary>Entrada de criação de missão manual (RF09). O XP é validado no backend contra a faixa (RN04).</summary>
public sealed record CriarMissaoRequest(
    string Titulo,
    EsforcoMissao Esforco,
    int Xp,
    string? Descricao = null,
    int? TopicoId = null);

/// <summary>Faixa de XP permitida por esforço (RN04) — o front usa para montar o formulário e validar antes de enviar.</summary>
public sealed record FaixaEsforcoDto(EsforcoMissao Esforco, int XpMinimo, int XpMaximo);

/// <summary>Pedido de missão a partir de texto livre (RF10) — a IA estrutura, o backend valida o XP.</summary>
public sealed record SugerirMissaoRequest(string Texto);

/// <summary>Mapeamentos de missão → DTO.</summary>
public static class MapeamentosMissaoDto
{
    public static MissaoDto ParaDto(this Missao m) =>
        new(m.Id, m.Titulo, m.Descricao, m.Tipo, m.Status, m.XpRecompensa, m.GoldRecompensa, m.DataAlvo, m.ConcluidaEm);
}
