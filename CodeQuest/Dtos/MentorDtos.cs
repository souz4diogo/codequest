using CodeQuest.Common;
using System.Text.Json;
using CodeQuest.Models;
using CodeQuest.Services.Ai;

namespace CodeQuest.Dtos;

/// <summary>Pergunta ao mentor IA (RF22). Tópico é opcional — mentor também tira dúvidas soltas.</summary>
public sealed record PerguntarRequest(int? TopicoId, string Pergunta);

/// <summary>Marca/desmarca uma dúvida para revisão futura (RF23).</summary>
public sealed record MarcarRevisaoRequest(bool Marcada);

/// <summary>Dúvida + resposta do mentor, prontas para exibir no chat.</summary>
public sealed record DuvidaDto(
    int Id,
    int? TopicoId,
    string? TopicoNome,
    string Pergunta,
    string Resposta,
    bool MarcadaParaRevisao,
    DateTime Data);

public static class MapeamentosMentorDto
{

    public static DuvidaDto ParaDto(this Duvida d) =>
        new(d.Id, d.TopicoId, d.Topico?.Nome, d.Pergunta, TextoResposta(d.RespostaJson),
            d.MarcadaParaRevisao, d.Data);

    private static string TextoResposta(string? respostaJson)
    {
        if (respostaJson is null) return string.Empty;
        try
        {
            return JsonSerializer.Deserialize<RespostaMentorIa>(respostaJson, JsonPadrao.Opcoes)?.Resposta ?? string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }
}
